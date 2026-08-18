[CmdletBinding()]
param(
    [string]$ConfigPath = 'C:\ProgramData\Peletnapechkai\Secrets\automation-worker.json'
)

$ErrorActionPreference = 'Stop'
$logRoot = 'C:\ProgramData\Peletnapechkai\Logs\AutomationWorker'
. (Join-Path $PSScriptRoot 'BoeclAutomationRecovery.ps1')
New-Item -ItemType Directory -Path $logRoot -Force | Out-Null
$mutex = [Threading.Mutex]::new($false, 'Global\BOECL-Codex-Automation-Worker')
$mutexAcquired = $false
try { $mutexAcquired = $mutex.WaitOne(0) }
catch [Threading.AbandonedMutexException] { $mutexAcquired = $true }
if (-not $mutexAcquired) { $mutex.Dispose(); exit 0 }

function Invoke-CodexProcess {
    param(
        [Parameter(Mandatory)] [string]$Executable,
        [Parameter(Mandatory)] [string[]]$Arguments,
        [Parameter(Mandatory)] [string]$InputText,
        [Parameter(Mandatory)] [string]$OutputPath,
        [Parameter(Mandatory)] [string]$ErrorPath,
        [scriptblock]$ShouldContinue,
        [int]$TimeoutMinutes = 60,
        [int]$MaximumAttempts = 2
    )

    $inputPath = "$OutputPath.stdin"
    try {
        $InputText | Set-Content -LiteralPath $inputPath -Encoding utf8
        $argumentLine = ($Arguments | ForEach-Object { '"' + $_.Replace('"', '\"') + '"' }) -join ' '
        for ($attempt = 1; $attempt -le $MaximumAttempts; $attempt++) {
            Remove-Item -LiteralPath $OutputPath, $ErrorPath -Force -ErrorAction SilentlyContinue
            $process = Start-Process -FilePath $Executable -ArgumentList $argumentLine -NoNewWindow -PassThru `
                -RedirectStandardInput $inputPath -RedirectStandardOutput $OutputPath -RedirectStandardError $ErrorPath
            # Materialize the native process handle before waiting. Windows PowerShell
            # can otherwise lose the handle and expose a null ExitCode for long-running
            # redirected processes even though they completed successfully.
            $null = $process.Handle
            $deadline = [DateTimeOffset]::UtcNow.AddMinutes($TimeoutMinutes)
            $completed = $false
            while ([DateTimeOffset]::UtcNow -lt $deadline) {
                if ($process.WaitForExit(5000)) { $completed = $true; break }
                if ($ShouldContinue -and -not (& $ShouldContinue)) {
                    & taskkill.exe /PID $process.Id /T /F | Out-Null
                    return 1223
                }
            }
            if ($completed) {
                # Start-Process with redirected streams may report HasExited before the
                # asynchronous stream readers and native process handle are finalized.
                # A parameterless second wait is required before ExitCode is reliable.
                $process.WaitForExit()
                $process.Refresh()
                $exitCode = $process.ExitCode
                if ($null -eq $exitCode) {
                    throw "Codex süreci tamamlandı ancak çıkış kodu alınamadı; sonuç teslim edilmedi."
                }
                return [int]$exitCode
            }

            & taskkill.exe /PID $process.Id /T /F | Out-Null
            "$(Get-Date -Format o) Codex süre sınırını aştı; deneme $attempt/$MaximumAttempts sonlandırıldı." |
                Add-Content -LiteralPath $ErrorPath -Encoding utf8
            if ($attempt -lt $MaximumAttempts) { Start-Sleep -Seconds 5 }
        }
        throw "Codex $MaximumAttempts denemede de $TimeoutMinutes dakikalık süre sınırını aştı."
    }
    finally {
        Remove-Item -LiteralPath $inputPath -Force -ErrorAction SilentlyContinue
    }
}

function Send-WorkerHeartbeat {
    param([string]$ApiUrl, [hashtable]$Headers, [string]$JobId, [string]$Message)
    $payload = @{ message = $Message } | ConvertTo-Json
    Invoke-RestMethod -Method Post -Uri "$ApiUrl/api/v1/internal/automation-worker/$JobId/heartbeat" -Headers $Headers `
        -ContentType 'application/json; charset=utf-8' -Body ([Text.Encoding]::UTF8.GetBytes($payload)) | Out-Null
}

function Invoke-BoeclApiRequest {
    param(
        [Parameter(Mandatory)] [ValidateSet('Get', 'Post')] [string]$Method,
        [Parameter(Mandatory)] [string]$Uri,
        [Parameter(Mandatory)] [hashtable]$Headers,
        [string]$ContentType,
        [byte[]]$Body
    )

    try {
        $parameters = @{ Method = $Method; Uri = $Uri; Headers = $Headers }
        if ($ContentType) { $parameters.ContentType = $ContentType }
        if ($null -ne $Body) { $parameters.Body = $Body }
        return Invoke-RestMethod @parameters
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        $responseBody = ''
        try {
            $stream = $_.Exception.Response.GetResponseStream()
            if ($stream) {
                $reader = [IO.StreamReader]::new($stream, [Text.Encoding]::UTF8)
                try { $responseBody = $reader.ReadToEnd() } finally { $reader.Dispose() }
            }
        }
        catch { }
        $safeDetail = if ([string]::IsNullOrWhiteSpace($responseBody)) { $_.Exception.Message } else { $responseBody }
        throw "BOECL API istegi basarisiz (HTTP $statusCode, $Method $([Uri]$Uri).AbsolutePath): $safeDetail"
    }
}

try {
    $config = Get-Content -LiteralPath $ConfigPath -Raw | ConvertFrom-Json
    $env:CODEX_HOME = [string]$config.codexHome
    $repositoryPath = [string]$config.repositoryPath
    if (-not (Test-Path -LiteralPath (Join-Path $repositoryPath 'package.json') -PathType Leaf)) {
        throw "Worker depo klasörü geçersiz veya package.json bulunamadı: $repositoryPath"
    }
    Set-Location -LiteralPath $repositoryPath
    $headers = @{ 'X-BOECL-Worker-Token' = [string]$config.workerToken }
    try {
        $job = Invoke-RestMethod -Method Post -Uri "$($config.apiUrl)/api/v1/internal/automation-worker/claim" -Headers $headers
    }
    catch {
        if ($_.Exception.Response.StatusCode.value__ -eq 204) { exit 0 }
        throw
    }
    if (-not $job.id) { exit 0 }

    $jobId = [string]$job.id
    $controlCheck = {
        try {
            $control = Invoke-RestMethod -Method Get -Uri "$($config.apiUrl)/api/v1/internal/automation-worker/$jobId/control" -Headers $headers
            return [bool]$control.shouldContinue
        }
        catch { return $true }
    }
    $jobLog = Join-Path $logRoot "$jobId.jsonl"
    $stderrLog = Join-Path $logRoot "$jobId-stderr.log"
    $lastMessage = Join-Path $logRoot "$jobId-result.txt"
    $savedErrorPreference = $ErrorActionPreference
    Send-WorkerHeartbeat -ApiUrl ([string]$config.apiUrl) -Headers $headers -JobId $jobId -Message "Worker işi aldı; aday kapsamı hazırlanıyor."
    if ($job.type -in @('ContentTranslation', 'SeoLocalization', 'ReadyContentGeneration', 'CategoryLocalization')) {
        $batch = 0
        $processed = 0
        $runId = Get-Date -Format 'yyyyMMdd-HHmmss'
        do {
            $candidateSet = Invoke-RestMethod -Method Get -Uri "$($config.apiUrl)/api/v1/internal/automation-worker/$jobId/candidates" -Headers $headers
            $candidateKind = [string]$candidateSet.kind
            $candidates = @($candidateSet.candidates)
            $candidateCount = if ($candidateKind -eq 'generation') { [int]$candidateSet.requestedCount } else { $candidates.Count }
            if ($candidateKind -eq 'complete' -or $candidateCount -eq 0) { break }
            $batch++
            $operationName = if ($candidateKind -eq 'generation') { 'araştırma ve Türkçe makale üretimi' } elseif ($candidateKind -eq 'translation') { 'içerik çevirisi' } elseif ($candidateKind -eq 'category') { 'kategori çevirisi' } else { 'SEO yerelleştirmesi' }
            Send-WorkerHeartbeat -ApiUrl ([string]$config.apiUrl) -Headers $headers -JobId $jobId -Message "Paket ${batch}: $candidateCount kayıt için $operationName Codex tarafından işleniyor."
            $batchResult = Join-Path $logRoot "$jobId-$runId-batch-$batch-result.json"
            $batchLog = Join-Path $logRoot "$jobId-$runId-batch-$batch.jsonl"
            $batchError = Join-Path $logRoot "$jobId-$runId-batch-$batch-stderr.log"
            $schemaRelative = if ($candidateKind -eq 'generation') { 'ops\automation\ready-content-output.schema.json' } elseif ($candidateKind -eq 'translation') { 'ops\automation\translation-output.schema.json' } elseif ($candidateKind -eq 'category') { 'ops\automation\category-translation-output.schema.json' } else { 'ops\automation\seo-output.schema.json' }
            $schema = Join-Path ([string]$config.repositoryPath) $schemaRelative
            $candidateJson = $candidateSet | ConvertTo-Json -Depth 8 -Compress
            $requestFingerprint = Get-BoeclRequestFingerprint -RequestJson $candidateJson
            $instruction = if ($candidateKind -eq 'generation') {
                "Canlı web aramasını kullan. Seçilen Türkçe kategori ve içerik türü için güncel, popüler ve güvenilir Türkçe/global yayınları ayrıntılı araştır. İstenen sayıda birbirinden ve existing listesinden belirgin biçimde farklı, en az 2500 karakter gövdeli, özgün Türkçe makale yaz. Kopyalama yapma; en az iki gerçek araştırma kaynağının doğrudan URL'sini her makalede bildir. Başlık/özet/slug tekrar etmesin. autoSeo doğruysa SEO alanlarını doldur, değilse null gönder. includeImages doğruysa kapak için özgün Türkçe imageAltText ve kısa Türkçe imageSearchQuery; gövde için birbirinden farklı tam iki Türkçe inlineImageAltTexts ve iki kısa Türkçe inlineImageQueries üret. Her sorgu başlık, özet veya kategoriyle ortak somut konu nesneleri içeren gerçek bir sahne tarif etsin; soyut/dekoratif şablon kullanma ve üç sahneyi birbirinden belirgin biçimde farklı kur. Tüm sorgular no text, no letters, no numbers, no symbols, no logo, no watermark şartını taşısın. includeImages yanlışsa tüm görsel alanlarını null gönder. Yalnız şemaya uyan JSON döndür.`r`n$candidateJson"
            } elseif ($candidateKind -eq 'translation') {
                "Aşağıdaki yayımlanmış Türkçe kaynakları belirtilen hedef dile doğal, eksiksiz ve editoryal kalitede çevir. HTML yapısını koru; yeni bilgi ekleme. Slug yalnız küçük ASCII harf, rakam ve tire içersin. Kimlikleri ve locale değerlerini aynen koru. Yalnız şemaya uyan JSON döndür. Doğrulanan sonuçlar doğrudan yayımlanacak.`r`n$candidateJson"
            } elseif ($candidateKind -eq 'category') {
                "Aşağıdaki Türkçe kategorileri belirtilen hedef dile doğal biçimde çevir. sourceCategoryId ve locale değerlerini aynen koru. Adı hedef dilde kısa ve anlaşılır yaz; slug yalnız küçük ASCII harf, rakam ve tire içersin. Yalnız şemaya uyan JSON döndür.`r`n$candidateJson"
            } else {
                "Aşağıdaki yayımlanmış içerikler için kendi dilinde doğal SEO başlığı ve açıklaması üret. İçerikte olmayan iddia ekleme; articleId değerini aynen koru. Yalnız şemaya uyan JSON döndür.`r`n$candidateJson"
            }
            if ($candidateKind -eq 'generation' -and -not [string]::IsNullOrWhiteSpace([string]$candidateSet.contentBrief)) {
                $instruction = "ZORUNLU ICERIK BRIFI: $([string]$candidateSet.contentBrief)`r`nBu brif genel uretim kurallarindan onceliklidir ve eksiksiz uygulanmalidir.`r`n$instruction"
            }
            $codexArguments = @()
            if ($candidateKind -eq 'generation') { $codexArguments += '--search' }
            $codexArguments += @('exec', '--ephemeral', '--json', '--sandbox', 'read-only', '--cd', [string]$config.repositoryPath, '--output-schema', $schema, '--output-last-message', $batchResult)
            $codexArguments += '-'
            $recovered = $false
            if ($candidateKind -eq 'generation') {
                $existingSlugs = @($candidateSet.existing | ForEach-Object { [string]$_.slug })
                $previousResultPath = Find-BoeclRecoveredResult -LogRoot $logRoot -JobId $jobId -CurrentResultPath $batchResult -RequestFingerprint $requestFingerprint
                if ($previousResultPath) {
                    try {
                        $previousJson = Get-Content -LiteralPath $previousResultPath -Raw -Encoding UTF8
                        $previousPayload = $previousJson | ConvertFrom-Json
                        $previousItems = @($previousPayload.items)
                        $previousSlugs = @($previousItems | ForEach-Object { [string]$_.slug })
                        if ($previousItems.Count -eq $candidateCount -and
                            @($previousSlugs | Where-Object { $existingSlugs -contains $_ }).Count -eq 0) {
                            Set-Content -LiteralPath $batchResult -Value $previousJson -Encoding UTF8
                            $recovered = $true
                        }
                    }
                    catch {
                        $recovered = $false
                    }
                }
            }
            if (-not $recovered) {
                Save-BoeclRecoveryMetadata -ResultPath $batchResult -RequestFingerprint $requestFingerprint
            }
            $codexExitCode = if ($recovered) { 0 } else { Invoke-CodexProcess -Executable ([string]$config.codexPath) -Arguments $codexArguments -InputText $instruction -OutputPath $batchLog -ErrorPath $batchError -ShouldContinue $controlCheck -TimeoutMinutes 60 -MaximumAttempts 2 }
            if ($codexExitCode -eq 1223) { exit 0 }
            if ($codexExitCode -ne 0) { throw "Codex yapılandırılmış içerik grubunu tamamlayamadı (batch $batch, exit $codexExitCode)." }
            Send-WorkerHeartbeat -ApiUrl ([string]$config.apiUrl) -Headers $headers -JobId $jobId -Message "Paket ${batch}: Codex çıktısı tamamlandı; şema ve API doğrulamasına gönderiliyor."
            $resultJson = Get-Content -LiteralPath $batchResult -Raw -Encoding UTF8
            $parsedResult = $resultJson | ConvertFrom-Json
            if (@($parsedResult.items).Count -ne $candidateCount) { throw "Codex aday sayısını eksik döndürdü (batch $batch)." }
            $payloadBase64 = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($resultJson))
            $submitPath = if ($candidateKind -eq 'generation') { 'generated-content' } elseif ($candidateKind -eq 'translation') { 'translations' } elseif ($candidateKind -eq 'category') { 'category-translations' } else { 'seo-drafts' }
            $submitBody = @{ payloadBase64 = $payloadBase64 } | ConvertTo-Json
            $submitBytes = [Text.Encoding]::UTF8.GetBytes($submitBody)
            Invoke-BoeclApiRequest -Method Post -Uri "$($config.apiUrl)/api/v1/internal/automation-worker/$jobId/$submitPath" -Headers $headers -ContentType 'application/json; charset=utf-8' -Body $submitBytes | Out-Null
            $processed += $candidateCount
            Send-WorkerHeartbeat -ApiUrl ([string]$config.apiUrl) -Headers $headers -JobId $jobId -Message "Paket ${batch} teslim edildi; yeni adaylar ve kalan iş hesaplanıyor."
        } while ($true)
        "## Yapılandırılmış otomasyon sonucu`r`n`r`n- İş türü: $($job.type)`r`n- İşlenen kayıt: $processed`r`n- Yayın durumu: Doğrulanan içerik ve çeviriler yayımlandı`r`n- Araştırma: Canlı web araması ve kayıtlı kaynak URL'leri" | Set-Content -LiteralPath $lastMessage -Encoding UTF8
    }
    else {
        $codexArguments = @('exec', '--ephemeral', '--json', '--sandbox', 'danger-full-access', '--cd', [string]$config.repositoryPath, '--output-last-message', $lastMessage, '-')
        $codexExitCode = Invoke-CodexProcess -Executable ([string]$config.codexPath) -Arguments $codexArguments -InputText ([string]$job.prompt) -OutputPath $jobLog -ErrorPath $stderrLog -ShouldContinue $controlCheck -TimeoutMinutes 90 -MaximumAttempts 2
        if ($codexExitCode -eq 1223) { exit 0 }
        if ($codexExitCode -ne 0) {
            $stderrTail = if (Test-Path -LiteralPath $stderrLog) { (Get-Content -LiteralPath $stderrLog -Tail 8) -join ' ' } else { '' }
            throw "Codex exited with code $codexExitCode. $stderrTail"
        }
    }

    $qualityLog = Join-Path $logRoot "$jobId-quality.log"
    $checks = @(
        @{ Name = 'Locale bütünlüğü'; Command = 'npm.cmd'; Arguments = @('run', 'check:locales') },
        @{ Name = 'Web lint'; Command = 'npm.cmd'; Arguments = @('run', 'lint') },
        @{ Name = 'Web tip denetimi'; Command = 'npm.cmd'; Arguments = @('run', 'typecheck') },
        @{ Name = 'Web üretim derlemesi'; Command = 'npm.cmd'; Arguments = @('run', 'build:web') },
        @{ Name = 'API testleri'; Command = 'dotnet.exe'; Arguments = @('test', 'Peletnapechkai.slnx', '--configuration', 'Release') },
        @{ Name = '.NET Release derlemesi'; Command = 'dotnet.exe'; Arguments = @('build', 'Peletnapechkai.slnx', '--configuration', 'Release') },
        @{ Name = 'Staging sağlık kontrolü'; Command = 'powershell.exe'; Arguments = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', 'ops\windows\Test-StagingHealth.ps1') },
        @{ Name = 'Production sağlık kontrolü'; Command = 'powershell.exe'; Arguments = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', 'ops\windows\Test-ProductionHealth.ps1') }
    )
    $qualityResults = [Collections.Generic.List[string]]::new()
    foreach ($check in $checks) {
        if (-not (& $controlCheck)) { exit 0 }
        Send-WorkerHeartbeat -ApiUrl ([string]$config.apiUrl) -Headers $headers -JobId $jobId -Message "Kalite kapısı çalışıyor: $($check.Name)."
        "[$(Get-Date -Format o)] START $($check.Name)" | Add-Content -LiteralPath $qualityLog -Encoding UTF8
        $ErrorActionPreference = 'Continue'
        $checkArguments = $check.Arguments
        & $check.Command @checkArguments 2>&1 | Add-Content -LiteralPath $qualityLog -Encoding UTF8
        $checkExitCode = $LASTEXITCODE
        $ErrorActionPreference = $savedErrorPreference
        if ($checkExitCode -ne 0) { throw "Kalite kapısı başarısız: $($check.Name) (exit $checkExitCode). Ayrıntı: $qualityLog" }
        $qualityResults.Add("- $($check.Name): Başarılı")
    }

    $report = if (Test-Path -LiteralPath $lastMessage) { Get-Content -LiteralPath $lastMessage -Raw -Encoding UTF8 } else { 'Codex işi tamamladı.' }
    $commit = (& git.exe -C ([string]$config.repositoryPath) rev-parse HEAD).Trim()
    $workingChanges = @(& git.exe -C ([string]$config.repositoryPath) status --short).Count
    $report += "`r`n`r`n## Zorunlu doğrulama kapıları`r`n`r`n$($qualityResults -join "`r`n")`r`n- Commit: $commit`r`n- Ortam: staging + production`r`n- Çalışma ağacı değişiklik sayısı: $workingChanges`r`n- Doğrulama zamanı: $((Get-Date).ToString('o'))"
    $reportBase64 = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($report))
    $body = @{ message = "Codex işi tamamladı; tüm zorunlu kalite kapıları geçti."; reportBase64 = $reportBase64 } | ConvertTo-Json
    $bodyBytes = [Text.Encoding]::UTF8.GetBytes($body)
    Invoke-RestMethod -Method Post -Uri "$($config.apiUrl)/api/v1/internal/automation-worker/$jobId/complete" -Headers $headers -ContentType 'application/json; charset=utf-8' -Body $bodyBytes | Out-Null
}
catch {
    $message = $_.Exception.Message
    if ($jobId) {
        try {
            if ($message.Length -gt 1800) { $message = $message.Substring(0, 1800) }
            $body = @{ message = $message } | ConvertTo-Json
            $bodyBytes = [Text.Encoding]::UTF8.GetBytes($body)
            Invoke-RestMethod -Method Post -Uri "$($config.apiUrl)/api/v1/internal/automation-worker/$jobId/fail" -Headers $headers -ContentType 'application/json; charset=utf-8' -Body $bodyBytes | Out-Null
        }
        catch { }
    }
    "$(Get-Date -Format o) job=$jobId $message" | Add-Content -LiteralPath (Join-Path $logRoot 'worker-errors.log')
    exit 1
}
finally {
    $mutex.ReleaseMutex()
    $mutex.Dispose()
}

[CmdletBinding()]
param(
    [string]$ConfigPath = 'C:\ProgramData\Peletnapechkai\Secrets\automation-worker.json'
)

$ErrorActionPreference = 'Stop'
$logRoot = 'C:\ProgramData\Peletnapechkai\Logs\AutomationWorker'
New-Item -ItemType Directory -Path $logRoot -Force | Out-Null
$mutex = [Threading.Mutex]::new($false, 'Global\BOECL-Codex-Automation-Worker')
if (-not $mutex.WaitOne(0)) { exit 0 }

function Invoke-CodexProcess {
    param(
        [Parameter(Mandatory)] [string]$Executable,
        [Parameter(Mandatory)] [string[]]$Arguments,
        [Parameter(Mandatory)] [string]$InputText,
        [Parameter(Mandatory)] [string]$OutputPath,
        [Parameter(Mandatory)] [string]$ErrorPath,
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
            if ($process.WaitForExit($TimeoutMinutes * 60 * 1000)) {
                return $process.ExitCode
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

try {
    $config = Get-Content -LiteralPath $ConfigPath -Raw | ConvertFrom-Json
    $env:CODEX_HOME = [string]$config.codexHome
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
    $jobLog = Join-Path $logRoot "$jobId.jsonl"
    $stderrLog = Join-Path $logRoot "$jobId-stderr.log"
    $lastMessage = Join-Path $logRoot "$jobId-result.txt"
    $savedErrorPreference = $ErrorActionPreference
    if ($job.type -in @('ContentTranslation', 'SeoLocalization', 'ReadyContentGeneration')) {
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
            $batchResult = Join-Path $logRoot "$jobId-$runId-batch-$batch-result.json"
            $batchLog = Join-Path $logRoot "$jobId-$runId-batch-$batch.jsonl"
            $batchError = Join-Path $logRoot "$jobId-$runId-batch-$batch-stderr.log"
            $schemaRelative = if ($candidateKind -eq 'generation') { 'ops\automation\ready-content-output.schema.json' } elseif ($candidateKind -eq 'translation') { 'ops\automation\translation-output.schema.json' } else { 'ops\automation\seo-output.schema.json' }
            $schema = Join-Path ([string]$config.repositoryPath) $schemaRelative
            $candidateJson = $candidateSet | ConvertTo-Json -Depth 8 -Compress
            $instruction = if ($candidateKind -eq 'generation') {
                "Canlı web aramasını kullan. Seçilen Türkçe kategori ve içerik türü için güncel, popüler ve güvenilir Türkçe/global yayınları ayrıntılı araştır. İstenen sayıda birbirinden ve existing listesinden belirgin biçimde farklı, en az 2500 karakter gövdeli, özgün Türkçe makale yaz. Kopyalama yapma; en az iki gerçek araştırma kaynağının doğrudan URL'sini her makalede bildir. Başlık/özet/slug tekrar etmesin. autoSeo doğruysa SEO alanlarını doldur, değilse null gönder. includeImages doğruysa özgün ve açıklayıcı imageAltText yaz. Yalnız şemaya uyan JSON döndür.`r`n$candidateJson"
            } elseif ($candidateKind -eq 'translation') {
                "Aşağıdaki yayımlanmış Türkçe kaynakları belirtilen hedef dile doğal, eksiksiz ve editoryal kalitede çevir. HTML yapısını koru; yeni bilgi ekleme. Slug yalnız küçük ASCII harf, rakam ve tire içersin. Kimlikleri ve locale değerlerini aynen koru. Yalnız şemaya uyan JSON döndür. Doğrulanan sonuçlar doğrudan yayımlanacak.`r`n$candidateJson"
            } else {
                "Aşağıdaki yayımlanmış içerikler için kendi dilinde doğal SEO başlığı ve açıklaması üret. İçerikte olmayan iddia ekleme; articleId değerini aynen koru. Yalnız şemaya uyan JSON döndür.`r`n$candidateJson"
            }
            $codexArguments = @()
            if ($candidateKind -eq 'generation') { $codexArguments += '--search' }
            $codexArguments += @('exec', '--ephemeral', '--json', '--sandbox', 'read-only', '--cd', [string]$config.repositoryPath, '--output-schema', $schema, '--output-last-message', $batchResult)
            $codexArguments += '-'
            $codexExitCode = Invoke-CodexProcess -Executable ([string]$config.codexPath) -Arguments $codexArguments -InputText $instruction -OutputPath $batchLog -ErrorPath $batchError -TimeoutMinutes 60 -MaximumAttempts 2
            if ($codexExitCode -ne 0) { throw "Codex yapılandırılmış içerik grubunu tamamlayamadı (batch $batch, exit $codexExitCode)." }
            $resultJson = Get-Content -LiteralPath $batchResult -Raw -Encoding UTF8
            $parsedResult = $resultJson | ConvertFrom-Json
            if (@($parsedResult.items).Count -ne $candidateCount) { throw "Codex aday sayısını eksik döndürdü (batch $batch)." }
            $payloadBase64 = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($resultJson))
            $submitPath = if ($candidateKind -eq 'generation') { 'generated-content' } elseif ($candidateKind -eq 'translation') { 'translations' } else { 'seo-drafts' }
            $submitBody = @{ payloadBase64 = $payloadBase64 } | ConvertTo-Json
            $submitBytes = [Text.Encoding]::UTF8.GetBytes($submitBody)
            Invoke-RestMethod -Method Post -Uri "$($config.apiUrl)/api/v1/internal/automation-worker/$jobId/$submitPath" -Headers $headers -ContentType 'application/json; charset=utf-8' -Body $submitBytes | Out-Null
            $processed += $candidateCount
        } while ($true)
        "## Yapılandırılmış otomasyon sonucu`r`n`r`n- İş türü: $($job.type)`r`n- İşlenen kayıt: $processed`r`n- Yayın durumu: Doğrulanan içerik ve çeviriler yayımlandı`r`n- Araştırma: Canlı web araması ve kayıtlı kaynak URL'leri" | Set-Content -LiteralPath $lastMessage -Encoding UTF8
    }
    else {
        $codexArguments = @('exec', '--ephemeral', '--json', '--sandbox', 'danger-full-access', '--cd', [string]$config.repositoryPath, '--output-last-message', $lastMessage, '-')
        $codexExitCode = Invoke-CodexProcess -Executable ([string]$config.codexPath) -Arguments $codexArguments -InputText ([string]$job.prompt) -OutputPath $jobLog -ErrorPath $stderrLog -TimeoutMinutes 90 -MaximumAttempts 2
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

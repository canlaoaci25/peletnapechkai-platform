[CmdletBinding()]
param(
    [string]$ConfigPath = 'C:\ProgramData\Peletnapechkai\Secrets\automation-worker.json',
    [string]$StateRoot = 'C:\ProgramData\Peletnapechkai\Autonomous'
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'AutonomousCycleRecovery.ps1')
$statePath = Join-Path $StateRoot 'state.json'
$logRoot = Join-Path $StateRoot 'Logs'
if (-not (Test-Path -LiteralPath $statePath)) { exit 0 }
$state = Get-Content -Raw -LiteralPath $statePath -Encoding UTF8 | ConvertFrom-Json
if (-not $state.enabled) { exit 0 }
function Set-StateValue([string]$Name, $Value) { $state | Add-Member -NotePropertyName $Name -NotePropertyValue $Value -Force }
if (Test-BoeclUtcDeadlinePending -Deadline ([string]$state.nextRetryAt)) { exit 0 }
New-Item -ItemType Directory -Path $logRoot -Force | Out-Null
$mutex = [Threading.Mutex]::new($false, 'Global\BOECL-Autonomous-Improvement')
$mutexAcquired = $false
try { $mutexAcquired = $mutex.WaitOne(0) }
catch [Threading.AbandonedMutexException] { $mutexAcquired = $true }
if (-not $mutexAcquired) { $mutex.Dispose(); exit 0 }

try {
    if ([string]$state.currentStatus -eq 'Running' -and (Test-BoeclHeartbeatStale -Heartbeat ([string]$state.heartbeatAt))) {
        Set-StateValue 'recoveredFromCycle' ([int]$state.currentCycle)
        Set-StateValue 'automaticRecoveries' (([int]$state.automaticRecoveries) + 1)
        Set-StateValue 'recoveryState' 'RecoveredAbandonedRun'
    }
    $config = Get-Content -Raw -LiteralPath $ConfigPath -Encoding UTF8 | ConvertFrom-Json
    $repository = [IO.Path]::GetFullPath([string]$config.repositoryPath)
    if (-not (Test-Path -LiteralPath (Join-Path $repository 'AGENTS.md'))) { throw 'Yetkili BOECL deposu doğrulanamadı.' }
    if (@(& git.exe -C $repository status --porcelain).Count -gt 0) { throw 'Çalışma ağacı temiz değil; kullanıcı değişikliklerini korumak için çevrim atlandı.' }
    $baselineCommit = (& git.exe -C $repository rev-parse HEAD).Trim()
    $masterInstructionsPath = 'C:\Users\Administrator\Desktop\New Text Document.txt'
    if (-not (Test-Path -LiteralPath $masterInstructionsPath -PathType Leaf)) { throw 'Kullanici master otonom talimat dosyasi bulunamadi.' }
    $masterInstructionsInfo = Get-Item -LiteralPath $masterInstructionsPath
    if ($masterInstructionsInfo.Length -le 0 -or $masterInstructionsInfo.Length -gt 100KB) { throw 'Kullanici master otonom talimat dosyasi gecersiz boyutta.' }
    $masterInstructions = Get-Content -Raw -LiteralPath $masterInstructionsPath -Encoding UTF8
    $masterAuditMode = -not ($state.PSObject.Properties.Name -contains 'masterAuditCompleted' -and [bool]$state.masterAuditCompleted)

    $focuses = @(
        'ana sayfa, global navigasyon ve gorunur tasarim donusumu',
        'icerik kesfi, kategori mimarisi ve yeni Turkce taxonomy',
        'uyelik, etkilesim ve ziyaretciyi geri getiren urun ozellikleri',
        'admin paneli, editoryal verimlilik ve yonetilebilirlik',
        'Turkce icerik, SEO, kaynak kalitesi ve trafik buyumesi',
        'ceviri, locale butunlugu ve uluslararasi deneyim',
        'makale gorsellerinin konu uygunlugu ve yazisiz ozgun tasarimi',
        'otomasyon, hata kurtarma ve canli dagitim guvenilirligi'
    )
    $cycle = [int]$state.cycle + 1
    $focus = $focuses[($cycle - 1) % $focuses.Count]
    # Advance immediately so a failed deployment cannot trap every later run
    # on the same focus forever.
    $state.cycle = $cycle
    $stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
    $output = Join-Path $logRoot "$stamp-cycle-$cycle-result.txt"
    $events = Join-Path $logRoot "$stamp-cycle-$cycle.jsonl"
    $errors = Join-Path $logRoot "$stamp-cycle-$cycle-stderr.log"
    Set-StateValue 'currentCycle' $cycle
    Set-StateValue 'currentFocus' $focus
    Set-StateValue 'currentStatus' 'Running'
    Set-StateValue 'currentStartedAt' ([DateTimeOffset]::UtcNow.ToString('o'))
    Set-StateValue 'heartbeatAt' $state.currentStartedAt
    Set-StateValue 'recoveryState' 'Healthy'
    Set-StateValue 'currentEventLog' $events
    Set-StateValue 'currentResultLog' $output
    $state.updatedAt = $state.currentStartedAt
    $state | ConvertTo-Json | Set-Content -LiteralPath "$statePath.tmp" -Encoding UTF8
    Move-Item -LiteralPath "$statePath.tmp" -Destination $statePath -Force
    $prompt = @"
BOECL tam yetkili otonom geliştirme çevrimi $cycle. Bu çevrimin odağı: $focus.

Asagidaki CODEX MASTER INSTRUCTIONS metni kullanici tarafindan kalici ana talimat olarak verilmistir. Tamamini bu cevrimde uygula; cevrim odagi bu ana kurallari daraltmaz:

----- KULLANICI MASTER TALIMATLARI BASLANGICI -----
$masterInstructions
----- KULLANICI MASTER TALIMATLARI SONU -----

Bu master talimatlar icin ilk calistirma denetimi gerekli mi: $masterAuditMode. Deger True ise once tam analiz ve oncelikli ilk 20 gelistirme raporunu depo altinda kalici dokuman olarak olustur, sonra ayni cevrimde kanitlanan en kritik guvenli isi uygula. Deger False ise daha onceki denetimi ve backlog'u okuyarak siradaki faza devam et; ilk analiz adimini gereksiz yere tekrarlama.

Repo AGENTS.md kurallarını eksiksiz uygula. Sistemi incele, kanıtlanabilir en yüksek değerli ürün fazını seç, uygula ve regresyon testlerini ekle. Türkçe içerik, SEO, etkin dillere çeviri, taxonomy, yazısız konuya özel kapak ve gövde görselleri, API, admin, mobil, güvenlik ve operasyon bütünlüğünü birlikte geliştir. Kullanıcı veya başka süreç değişikliklerini silme. Sırları okuma veya raporlama. Veritabanı şemasını, indeksleri, ilişkileri ve uygulama verisini geliştirebilirsin; bunu yedek, migration, transaction, doğrulanan servis/API ve audit iziyle güvenli ve tekrarlanabilir yap. Kalite kapıları geçmezse commit/push/deploy yapma. Geçerse anlamlı commit oluştur ve origin/main dalına push et. Geri döndürülemez hesap, DNS, ödeme veya kimlik işlemi yapma. Sonuçta yapılanları, testleri, commit'i ve kalan riski Türkçe raporla.
Bu bir bakim botu degil, BOECL'in tamamini gelistiren tam yetkili urun, tasarim, yazilim, veri, editor, SEO ve yerellestirme ekibidir. Nihai hedef; Onedio, BBC, CNN, The Verge, Wired, Vox ve benzeri global yayinlarin kesif, hiz, guven, gorsel hiyerarsi, kategori mimarisi, uyelik ve cok dilli erisim guclerini arastirip BOECL kimligiyle daha iyi bir global icerik platformu kurmaktir. Tasarim veya metin kopyalama; prensipleri arastir, olc ve ozgun uygula. Her cevrim tek bir urun fazini uctan uca tamamlamali ve kullanici sayfayi actiginda gozle gorulur bir fark olusturmalidir. Sadece CSS ayrintisi, lazy-loading, aria, test, refactor, dokuman veya altyapi degisikligi tek basina cevrim basarisi olamaz; bunlar gorunur fazin destekleyici parcalari olabilir.

Tasarim yetkin tamdir: mevcut arayuzu korumak zorunda degilsin. Kanita dayali daha iyi bir sonuc icin bilgi mimarisini, ana sayfayi, header/menu yapisini, logo olcegini, gridleri, kartlari, tipografiyi, renkleri, bosluk sistemini, animasyonlari, dark/aydinlik temayi, makale sayfasini, arsivleri, uyelik ekranlarini ve admin panelini yeniden tasarlayabilirsin. Parca parca stil yamasi yerine tutarli design tokenlari ve tekrar kullanilan component sistemi kur. Masaustu, tablet ve mobil gorunumleri ayni fazda tamamla. BOECL marka adi, domaini, yazisiz gorsel ilkesi ve Turkce ana yayin dili korunur.

Yeni icerik fikirleri uygulama yetkin de tamdir. Canli trend, arama niyeti, mevcut arsiv bosluklari ve global yayin desenlerinden kanitli firsat bulursan yeni Turkce kategori, alt kategori, etiket, icerik turu, seri/dosya, rehber, liste, test/anket, karsilastirma, gundem veya evergreen merkezleri tasarlayip kod, veri modeli, admin yonetimi, SEO, ceviri, gorsel ve ana sayfa kesif alanlariyla birlikte canliya alabilirsin. Fikir yalniz raporda kalmasin; kalite kapilarini gecen uygulanmis urun sonucu olsun. Dusuk kaliteli, tekrar eden veya kaynaksiz toplu icerik uretme.

Sistem kullanici durdurana kadar gelismeyi surdurur. Her cevrimde canli urun envanterini ve onceki raporlari okuyup kalici bir iyilestirme backlog'u olustur/guncelle; en yuksek etkili tamamlanmamis fazi sec. Ayni isi tekrarlama, anlamsiz commit sayisi uretme ve mevcut kaliteyi geriye goturme. Bir hedef tamamlaninca siradaki en yuksek etkili hedefe gec; yapilacak is kalmadigini varsayma, global rakipler, kullanici deneyimi, veri ve performans kanitlariyla yeni firsat ara.

Her cevrimin zorunlu akisi:
1. Canli siteyi, admini, son 20 commit'i ve kalici yol haritasini incele; tekrar eden mikro isi secme.
2. O odak icin ziyaretcinin veya yoneticinin gorecegi bir once/sonra hedefi yaz. Ana sayfa/modul/akis/taxonomy/icerik sunumu gibi butun bir yuzeyi ele al.
3. Tasarim fazinda masaustu ve mobil hiyerarsi, bosluk, tipografi, kartlar, navigasyon, dark/aydinlik tema ve erisilebilirligi birlikte tamamla. Global kaliteli yayinlardan yalniz desen ve prensip arastir; tasarimi kopyalama.
4. Icerik fazinda yeni Turkce kategori ihtiyacini gercek arsiv ve trendlerle denetle; kanitli ihtiyac varsa migration, seed, yetkili servis/API veya denetlenebilir veri operasyonuyla olustur, cevirilerini ve SEO baglarini tamamla. Veritabani degisikliklerinde once yedek al, transaction kullan, tekrar calistirilabilirlik ve rollback yolu sagla.
5. Gorsel fazinda soyut dekoratif sablonu basari sayma; kapak ve govde gorselleri makalenin somut konusu/sahnesiyle uyumlu, yazisiz ve birbirinden farkli olsun.
6. Kabul kriterlerini test et, anlamli tek faz commit'i olustur, GitHub'a push et, staging ve production'a deploy et. Canli URL'lerde sonucu dogrulamadan Completed yazma.
7. Raporun ilk satirlarinda kullanicinin nerede hangi gorunur farki gorecegini, canli URL'yi ve deploy sonucunu belirt.

Bir cevrimde gorunur urun sonucu cikaramiyorsan mikro commit uretme; nedeni Failed olarak raporla ve sonraki odaga gec. Deploy basarisizsa gelistirme tamamlanmis sayilmaz.
"@
    $env:CODEX_HOME = [string]$config.codexHome
    $inputPath = Join-Path $logRoot "$stamp-cycle-$cycle.stdin"
    $prompt | Set-Content -LiteralPath $inputPath -Encoding UTF8
    $arguments = @('--search','exec','--ephemeral','--json','--sandbox','danger-full-access','--cd',$repository,'--output-last-message',$output,'-')
    $argumentLine = ($arguments | ForEach-Object { '"' + $_.Replace('"', '\"') + '"' }) -join ' '
    $process = Start-Process -FilePath ([string]$config.codexPath) -ArgumentList $argumentLine -NoNewWindow -PassThru `
        -RedirectStandardInput $inputPath -RedirectStandardOutput $events -RedirectStandardError $errors
    $null = $process.Handle
    while (-not $process.WaitForExit(15000)) {
        Set-StateValue 'heartbeatAt' ([DateTimeOffset]::UtcNow.ToString('o'))
        $state.updatedAt = $state.heartbeatAt
        $state | ConvertTo-Json | Set-Content -LiteralPath "$statePath.tmp" -Encoding UTF8
        Move-Item -LiteralPath "$statePath.tmp" -Destination $statePath -Force
    }
    $process.Refresh()
    $exitCode = $process.ExitCode
    Remove-Item -LiteralPath $inputPath -Force -ErrorAction SilentlyContinue
    if ($exitCode -ne 0) { throw "Codex çevrimi başarısız oldu (exit $exitCode)." }

    $checks = @(
        @{ Command='npm.cmd'; Arguments=@('run','check:locales') },
        @{ Command='npm.cmd'; Arguments=@('run','lint') },
        @{ Command='npm.cmd'; Arguments=@('run','typecheck') },
        @{ Command='npm.cmd'; Arguments=@('run','build:web') },
        @{ Command='dotnet.exe'; Arguments=@('test','Peletnapechkai.slnx','--configuration','Release') },
        @{ Command='dotnet.exe'; Arguments=@('build','Peletnapechkai.slnx','--configuration','Release') }
    )
    Push-Location $repository
    try {
        foreach ($check in $checks) { & $check.Command @($check.Arguments); if ($LASTEXITCODE -ne 0) { throw "Otonom kalite kapısı başarısız: $($check.Command)." } }
    } finally { Pop-Location }

    # Never deploy an incomplete standalone tree. A missing fallback module was
    # previously discovered only after the live swap had started.
    $fallback = Join-Path $repository 'apps\web\.next\standalone\node_modules\next\dist\lib\fallback.js'
    if (-not (Test-Path -LiteralPath $fallback -PathType Leaf)) {
        Push-Location $repository
        try { & npm.cmd run build:web; if ($LASTEXITCODE -ne 0) { throw 'Web release artifact rebuild failed.' } }
        finally { Pop-Location }
    }
    if (-not (Test-Path -LiteralPath $fallback -PathType Leaf)) { throw 'Web standalone release artifact is incomplete.' }

    $finalCommit = (& git.exe -C $repository rev-parse HEAD).Trim()
    $changedFiles = if ($finalCommit -ne $baselineCommit) { @(& git.exe -C $repository diff --name-only $baselineCommit $finalCommit) } else { @() }
    if (@(& git.exe -C $repository status --porcelain).Count -gt 0) { throw 'Otonom çevrim temiz olmayan çalışma ağacı bıraktı; dağıtım durduruldu.' }
    if ($changedFiles | Where-Object { $_ -like 'apps/api/*' -or $_ -like 'tests/api/*' }) {
        & (Join-Path $repository 'ops\windows\Backup-PostgreSql.ps1')
        if ($LASTEXITCODE -ne 0) { throw 'Dağıtım öncesi PostgreSQL yedeği başarısız.' }
        $hasMigrations = [bool]($changedFiles | Where-Object { $_ -like 'apps/api/Infrastructure/Persistence/Migrations/*' })
        if ($hasMigrations) { & (Join-Path $repository 'ops\windows\Update-BoeclDatabase.ps1') -Environment Staging -RepositoryPath $repository }
        & (Join-Path $repository 'ops\windows\Deploy-AspNetApiRelease.ps1') -Environment Staging -RepositoryPath $repository
        if ($hasMigrations) { & (Join-Path $repository 'ops\windows\Update-BoeclDatabase.ps1') -Environment Production -RepositoryPath $repository }
        & (Join-Path $repository 'ops\windows\Deploy-AspNetApiRelease.ps1') -Environment Production -RepositoryPath $repository
    }
    if ($changedFiles | Where-Object { $_ -like 'apps/web/*' -or $_ -like 'config/supported-locales.json' }) {
        & (Join-Path $repository 'ops\windows\Deploy-NextWebRelease.ps1') -Environment Staging
        & (Join-Path $repository 'ops\windows\Deploy-NextWebRelease.ps1') -Environment Production
    }

    $state.cycle = $cycle
    $state.lastRunAt = [DateTimeOffset]::UtcNow.ToString('o')
    $state.lastResult = 'Completed'
    Set-StateValue 'consecutiveFailures' 0
    Set-StateValue 'nextRetryAt' $null
    Set-StateValue 'recoveryState' 'Healthy'
    Set-StateValue 'heartbeatAt' $state.lastRunAt
    $state | Add-Member -NotePropertyName 'masterAuditCompleted' -NotePropertyValue $true -Force
    Set-StateValue 'currentStatus' 'Completed'
    $state.updatedAt = $state.lastRunAt
    $state | ConvertTo-Json | Set-Content -LiteralPath "$statePath.tmp" -Encoding UTF8
    Move-Item -LiteralPath "$statePath.tmp" -Destination $statePath -Force
}
catch {
    $state.lastRunAt = [DateTimeOffset]::UtcNow.ToString('o')
    $state.lastResult = "Failed: $($_.Exception.Message)"
    $failures = ([int]$state.consecutiveFailures) + 1
    $retryDelay = Get-BoeclRetryDelayMinutes -ConsecutiveFailures $failures
    Set-StateValue 'consecutiveFailures' $failures
    Set-StateValue 'lastFailureAt' $state.lastRunAt
    Set-StateValue 'nextRetryAt' ([DateTimeOffset]::UtcNow.AddMinutes($retryDelay).ToString('o'))
    Set-StateValue 'recoveryState' 'Backoff'
    Set-StateValue 'heartbeatAt' $state.lastRunAt
    Set-StateValue 'currentStatus' 'Failed'
    $state.updatedAt = $state.lastRunAt
    $state | ConvertTo-Json | Set-Content -LiteralPath "$statePath.tmp" -Encoding UTF8
    Move-Item -LiteralPath "$statePath.tmp" -Destination $statePath -Force
    "$(Get-Date -Format o) $($_.Exception.Message)" | Add-Content -LiteralPath (Join-Path $logRoot 'errors.log') -Encoding UTF8
    exit 1
}
finally {
    $mutex.ReleaseMutex()
    $mutex.Dispose()
}

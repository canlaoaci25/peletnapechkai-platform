[CmdletBinding()]
param(
    [string]$ConfigPath = 'C:\ProgramData\Peletnapechkai\Secrets\automation-worker.json',
    [string]$StateRoot = 'C:\ProgramData\Peletnapechkai\Autonomous'
)

$ErrorActionPreference = 'Stop'
$statePath = Join-Path $StateRoot 'state.json'
$logRoot = Join-Path $StateRoot 'Logs'
if (-not (Test-Path -LiteralPath $statePath)) { exit 0 }
$state = Get-Content -Raw -LiteralPath $statePath -Encoding UTF8 | ConvertFrom-Json
if (-not $state.enabled) { exit 0 }
New-Item -ItemType Directory -Path $logRoot -Force | Out-Null
$mutex = [Threading.Mutex]::new($false, 'Global\BOECL-Autonomous-Improvement')
if (-not $mutex.WaitOne(0)) { exit 0 }

try {
    $config = Get-Content -Raw -LiteralPath $ConfigPath -Encoding UTF8 | ConvertFrom-Json
    $repository = [IO.Path]::GetFullPath([string]$config.repositoryPath)
    if (-not (Test-Path -LiteralPath (Join-Path $repository 'AGENTS.md'))) { throw 'Yetkili BOECL deposu doğrulanamadı.' }
    if (@(& git.exe -C $repository status --porcelain).Count -gt 0) { throw 'Çalışma ağacı temiz değil; kullanıcı değişikliklerini korumak için çevrim atlandı.' }

    $focuses = @('iş mantığı ve API güvenilirliği','otomasyon ve hata kurtarma','Türkçe içerik, SEO ve kaynak kalitesi','çeviri ve locale bütünlüğü','erişilebilirlik, mobil tasarım ve performans','makale görsellerinin konu uygunluğu ve yazısız özgün tasarımı')
    $cycle = [int]$state.cycle + 1
    $focus = $focuses[($cycle - 1) % $focuses.Count]
    $stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
    $output = Join-Path $logRoot "$stamp-cycle-$cycle-result.txt"
    $events = Join-Path $logRoot "$stamp-cycle-$cycle.jsonl"
    $errors = Join-Path $logRoot "$stamp-cycle-$cycle-stderr.log"
    $prompt = @"
BOECL tam yetkili otonom geliştirme çevrimi $cycle. Bu çevrimin odağı: $focus.
Repo AGENTS.md kurallarını eksiksiz uygula. Sistemi incele, yalnız kanıtlanabilir en yüksek değerli ve sınırlı bir iyileştirme paketi seç, uygula ve regresyon testlerini ekle. Türkçe içerik, SEO, etkin dillere çeviri, taxonomy, yazısız konuya özel kapak ve gövde görselleri, API, admin, mobil, güvenlik ve operasyon bütünlüğünü birlikte koru. Kullanıcı veya başka süreç değişikliklerini silme. Sırları okuma veya raporlama. Veritabanına doğrudan içerik yazma; doğrulanan API/migration yollarını kullan. Kalite kapıları geçmezse commit/push/deploy yapma. Geçerse anlamlı commit oluştur ve origin/main dalına push et. Geri döndürülemez hesap, DNS, ödeme veya kimlik işlemi yapma. Sonuçta yapılanları, testleri, commit'i ve kalan riski Türkçe raporla.
"@
    $env:CODEX_HOME = [string]$config.codexHome
    $prompt | & ([string]$config.codexPath) --search exec --ephemeral --json --sandbox danger-full-access --cd $repository --output-last-message $output - 1> $events 2> $errors
    if ($LASTEXITCODE -ne 0) { throw "Codex çevrimi başarısız oldu (exit $LASTEXITCODE)." }

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

    $state.cycle = $cycle
    $state.lastRunAt = [DateTimeOffset]::UtcNow.ToString('o')
    $state.lastResult = 'Completed'
    $state.updatedAt = $state.lastRunAt
    $state | ConvertTo-Json | Set-Content -LiteralPath "$statePath.tmp" -Encoding UTF8
    Move-Item -LiteralPath "$statePath.tmp" -Destination $statePath -Force
}
catch {
    $state.lastRunAt = [DateTimeOffset]::UtcNow.ToString('o')
    $state.lastResult = "Failed: $($_.Exception.Message)"
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

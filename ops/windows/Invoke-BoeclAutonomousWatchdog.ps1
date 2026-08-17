[CmdletBinding()]
param(
    [string]$StateRoot = 'C:\ProgramData\Peletnapechkai\Autonomous',
    [string]$TaskName = 'BOECL Autonomous Improvement',
    [ValidateRange(1, 120)][int]$StaleAfterMinutes = 10
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'AutonomousWatchdogCore.ps1')

$statePath = Join-Path $StateRoot 'state.json'
$watchdogPath = Join-Path $StateRoot 'watchdog.json'
$logRoot = Join-Path $StateRoot 'Logs'
$logPath = Join-Path $logRoot 'watchdog.jsonl'
$mutex = [Threading.Mutex]::new($false, 'Global\BOECL-Autonomous-Watchdog')
$acquired = $false

function Write-WatchdogEvent {
    param([string]$Level, [string]$Event, [string]$Message)
    New-Item -ItemType Directory -Path $logRoot -Force | Out-Null
    [ordered]@{ at=[DateTimeOffset]::UtcNow.ToString('o'); level=$Level; event=$Event; message=$Message } |
        ConvertTo-Json -Compress | Add-Content -LiteralPath $logPath -Encoding UTF8
}

function Save-WatchdogState {
    param($Value)
    $temporary = "$watchdogPath.$([guid]::NewGuid().ToString('N')).tmp"
    $Value | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $temporary -Encoding UTF8
    Move-Item -LiteralPath $temporary -Destination $watchdogPath -Force
}

try {
    try { $acquired = $mutex.WaitOne(0) } catch [Threading.AbandonedMutexException] { $acquired = $true }
    if (-not $acquired) { exit 0 }
    if (-not (Test-Path -LiteralPath $statePath -PathType Leaf)) { Write-WatchdogEvent 'Warning' 'state_missing' 'Otonom durum dosyasi bulunamadi.'; exit 0 }

    $state = Get-Content -Raw -LiteralPath $statePath -Encoding UTF8 | ConvertFrom-Json
    if (-not [bool]$state.enabled) { Write-WatchdogEvent 'Info' 'disabled' 'Otonom mod kullanici tarafindan kapali.'; exit 0 }

    $task = Get-ScheduledTask -TaskName $TaskName -ErrorAction Stop
    $action = $task.Actions | Select-Object -First 1
    if ([string]$action.Execute -notmatch '(?i)powershell(?:\.exe)?$' -or [string]$action.Arguments -notmatch '(?i)(?:^|\s)-File\s+') {
        Write-WatchdogEvent 'Error' 'invalid_action' 'Ana gorev guvenli -File eylemini kullanmiyor; otomatik baslatma engellendi.'
        exit 2
    }

    $decision = Get-BoeclWatchdogDecision -Enabled ([bool]$state.enabled) -TaskState ([string]$task.State) `
        -CurrentStatus ([string]$state.currentStatus) -Heartbeat ([string]$state.heartbeatAt) `
        -NextRetryAt ([string]$state.nextRetryAt) -StaleAfterMinutes $StaleAfterMinutes

    $watchdog = if (Test-Path -LiteralPath $watchdogPath) {
        try { Get-Content -Raw -LiteralPath $watchdogPath -Encoding UTF8 | ConvertFrom-Json } catch { $null }
    } else { $null }
    $recoveries = if ($null -ne $watchdog -and $watchdog.PSObject.Properties.Name -contains 'recoveries') { @($watchdog.recoveries) } else { @() }

    if ($decision -in @('RecoverStaleRun','EnableAndStart','Start')) {
        $budget = Test-BoeclRecoveryBudget -Recoveries $recoveries
        if (-not $budget.Allowed) {
            Write-WatchdogEvent 'Critical' 'recovery_throttled' 'Otuz dakika icinde uc kurtarma denemesi yapildi; yeniden baslatma dongusu engellendi.'
            Save-WatchdogState ([ordered]@{ status='Throttled'; checkedAt=[DateTimeOffset]::UtcNow.ToString('o'); decision=$decision; recoveries=@($budget.Recent | ForEach-Object { $_.ToString('o') }) })
            exit 3
        }

        if ($decision -eq 'RecoverStaleRun' -and $task.State -in @('Running','Queued')) {
            Stop-ScheduledTask -TaskName $TaskName -ErrorAction Stop
            $limit = (Get-Date).AddSeconds(20)
            do { Start-Sleep -Milliseconds 500; $task = Get-ScheduledTask -TaskName $TaskName } while ($task.State -in @('Running','Queued') -and (Get-Date) -lt $limit)
            if ($task.State -in @('Running','Queued')) { throw 'Takili ana gorev yirmi saniye icinde durmadi.' }
        }
        if ($decision -eq 'EnableAndStart') { Enable-ScheduledTask -TaskName $TaskName | Out-Null }
        Start-ScheduledTask -TaskName $TaskName -ErrorAction Stop
        $now = [DateTimeOffset]::UtcNow
        $recent = @($budget.Recent | ForEach-Object { $_.ToString('o') }) + $now.ToString('o')
        Save-WatchdogState ([ordered]@{ status='Recovered'; checkedAt=$now.ToString('o'); decision=$decision; recoveries=$recent })
        Write-WatchdogEvent 'Warning' 'recovered' "Ana otonom gorev yeniden baslatildi: $decision."
        exit 0
    }

    Save-WatchdogState ([ordered]@{ status=$decision; checkedAt=[DateTimeOffset]::UtcNow.ToString('o'); decision=$decision; recoveries=@($recoveries) })
    Write-WatchdogEvent 'Info' 'checked' "Otonom sistem denetlendi: $decision."
}
catch {
    try { Write-WatchdogEvent 'Error' 'watchdog_failure' $_.Exception.Message } catch { }
    try { Write-EventLog -LogName Application -Source 'BOECL Autonomous Watchdog' -EntryType Error -EventId 4101 -Message $_.Exception.Message -ErrorAction SilentlyContinue } catch { }
    exit 1
}
finally {
    if ($acquired) { $mutex.ReleaseMutex() }
    $mutex.Dispose()
}

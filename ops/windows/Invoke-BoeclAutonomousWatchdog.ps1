[CmdletBinding()]
param(
    [string]$StateRoot = 'C:\ProgramData\Peletnapechkai\Autonomous',
    [string]$TaskName = 'BOECL Autonomous Improvement',
    [string]$ConfigPath = 'C:\ProgramData\Peletnapechkai\Secrets\automation-worker.json',
    [ValidateRange(1, 120)][int]$StaleAfterMinutes = 10
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'AutonomousWatchdogCore.ps1')
. (Join-Path $PSScriptRoot 'AutonomousRoadmap.ps1')

$statePath = Join-Path $StateRoot 'state.json'
$stateBackupPath = Join-Path $StateRoot 'state.last-good.json'
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

function Save-AutonomousState {
    param($Value)
    $temporary = "$statePath.$([guid]::NewGuid().ToString('N')).tmp"
    $Value | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $temporary -Encoding UTF8
    Move-Item -LiteralPath $temporary -Destination $statePath -Force
    Copy-Item -LiteralPath $statePath -Destination $stateBackupPath -Force
}

try {
    try { $acquired = $mutex.WaitOne(0) } catch [Threading.AbandonedMutexException] { $acquired = $true }
    if (-not $acquired) { exit 0 }
    if (-not (Test-Path -LiteralPath $statePath -PathType Leaf)) { Write-WatchdogEvent 'Warning' 'state_missing' 'Otonom durum dosyasi bulunamadi.'; exit 0 }

    try {
        $state = Get-Content -Raw -LiteralPath $statePath -Encoding UTF8 | ConvertFrom-Json
        Copy-Item -LiteralPath $statePath -Destination $stateBackupPath -Force
    }
    catch {
        if (-not (Test-Path -LiteralPath $stateBackupPath -PathType Leaf)) { throw }
        $state = Get-Content -Raw -LiteralPath $stateBackupPath -Encoding UTF8 | ConvertFrom-Json
        Save-AutonomousState $state
        Write-WatchdogEvent 'Warning' 'state_restored' 'Bozuk otonom durum dosyasi son saglam kopyadan geri yuklendi.'
    }
    if (-not [bool]$state.enabled) { Write-WatchdogEvent 'Info' 'disabled' 'Otonom mod kullanici tarafindan kapali.'; exit 0 }

    $task = Get-ScheduledTask -TaskName $TaskName -ErrorAction Stop
    $action = $task.Actions | Select-Object -First 1
    if ([string]$action.Execute -notmatch '(?i)powershell(?:\.exe)?$' -or [string]$action.Arguments -notmatch '(?i)(?:^|\s)-File\s+') {
        if ($task.State -in @('Running','Queued')) { throw 'Calisan ana gorevin eylemi guvenli bicimde onarilamaz.' }
        $config = Get-Content -Raw -LiteralPath $ConfigPath -Encoding UTF8 | ConvertFrom-Json
        $repository = [IO.Path]::GetFullPath([string]$config.repositoryPath)
        $cycleScript = Join-Path $repository 'ops\windows\Invoke-BoeclAutonomousCycle.ps1'
        if (-not (Test-Path -LiteralPath $cycleScript -PathType Leaf)) { throw 'Ana otonom cevrim betigi bulunamadi.' }
        $fixedAction = New-ScheduledTaskAction -Execute 'C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe' -Argument "-NoProfile -ExecutionPolicy Bypass -File `"$cycleScript`"" -WorkingDirectory $repository
        Set-ScheduledTask -TaskName $TaskName -Action $fixedAction | Out-Null
        $task = Get-ScheduledTask -TaskName $TaskName
        Write-WatchdogEvent 'Warning' 'action_repaired' 'Ana gorev guvenli -File eylemine geri alindi.'
    }

    if ([string]$state.currentStatus -eq 'Failed' -and [string]$state.lastResult -match 'yol haritasi en az .* gelecek adim') {
        $config = Get-Content -Raw -LiteralPath $ConfigPath -Encoding UTF8 | ConvertFrom-Json
        $repository = [IO.Path]::GetFullPath([string]$config.repositoryPath)
        $roadmapPath = Join-Path $repository 'docs\operations\autonomous-roadmap.json'
        $repairedRoadmap = @(Repair-BoeclAutonomousRoadmap -Path $roadmapPath)
        $now = [DateTimeOffset]::UtcNow.ToString('o')
        $state | Add-Member -NotePropertyName roadmap -NotePropertyValue $repairedRoadmap -Force
        $state | Add-Member -NotePropertyName consecutiveFailures -NotePropertyValue 0 -Force
        $state | Add-Member -NotePropertyName nextRetryAt -NotePropertyValue $null -Force
        $state | Add-Member -NotePropertyName currentStatus -NotePropertyValue 'Queued' -Force
        $state | Add-Member -NotePropertyName recoveryState -NotePropertyValue 'SelfHealedRoadmap' -Force
        $state | Add-Member -NotePropertyName automaticRecoveries -NotePropertyValue (([int]$state.automaticRecoveries)+1) -Force
        $state | Add-Member -NotePropertyName updatedAt -NotePropertyValue $now -Force
        Save-AutonomousState $state
        Write-WatchdogEvent 'Warning' 'roadmap_repaired' 'Eksik otonom yol haritasi otomatik tamamlandi ve backoff temizlendi.'
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

[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateSet('Start','Stop','Status')][string]$Action,
    [string]$Root = 'C:\ProgramData\Peletnapechkai\Autonomous'
)

$ErrorActionPreference = 'Stop'
$statePath = Join-Path $Root 'state.json'
New-Item -ItemType Directory -Path $Root -Force | Out-Null
$state = if (Test-Path -LiteralPath $statePath) {
    try { Get-Content -Raw -LiteralPath $statePath -Encoding UTF8 | ConvertFrom-Json } catch { $null }
} else { $null }
$payload = [ordered]@{
    enabled = if ($Action -eq 'Start') { $true } elseif ($Action -eq 'Stop') { $false } else { [bool]$state.enabled }
    cycle = if ($state -and $null -ne $state.cycle) { [int]$state.cycle } else { 0 }
    startedAt = if ($Action -eq 'Start') { [DateTimeOffset]::UtcNow.ToString('o') } else { $state.startedAt }
    stoppedAt = if ($Action -eq 'Stop') { [DateTimeOffset]::UtcNow.ToString('o') } else { $state.stoppedAt }
    lastRunAt = $state.lastRunAt
    lastResult = $state.lastResult
    consecutiveFailures = if ($Action -eq 'Start') { 0 } elseif ($state) { [int]$state.consecutiveFailures } else { 0 }
    automaticRecoveries = if ($state) { [int]$state.automaticRecoveries } else { 0 }
    recoveredFromCycle = $state.recoveredFromCycle
    recoveryState = if ($Action -eq 'Start') { 'Queued' } elseif ($Action -eq 'Stop') { 'Stopped' } else { $state.recoveryState }
    heartbeatAt = $state.heartbeatAt
    lastFailureAt = if ($Action -eq 'Start') { $null } else { $state.lastFailureAt }
    nextRetryAt = if ($Action -eq 'Start') { $null } else { $state.nextRetryAt }
    currentStatus = if ($Action -eq 'Start') { 'Queued' } elseif ($Action -eq 'Stop') { 'Stopping' } else { $state.currentStatus }
    currentCycle = $state.currentCycle
    currentFocus = $state.currentFocus
    currentStartedAt = $state.currentStartedAt
    currentEventLog = $state.currentEventLog
    currentResultLog = $state.currentResultLog
    roadmap = $state.roadmap
    githubPushPausedUntil = $state.githubPushPausedUntil
    updatedAt = [DateTimeOffset]::UtcNow.ToString('o')
}
if ($Action -ne 'Status') {
    $temporary = "$statePath.tmp"
    $payload | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $temporary -Encoding UTF8
    Move-Item -LiteralPath $temporary -Destination $statePath -Force
}
if ($Action -eq 'Start') {
    Enable-ScheduledTask -TaskName 'BOECL Autonomous Improvement' -ErrorAction SilentlyContinue | Out-Null
    Start-ScheduledTask -TaskName 'BOECL Autonomous Improvement' -ErrorAction SilentlyContinue
}
[pscustomobject]$payload

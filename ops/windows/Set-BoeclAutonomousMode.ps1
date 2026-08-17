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
    consecutiveFailures = if ($state) { [int]$state.consecutiveFailures } else { 0 }
    automaticRecoveries = if ($state) { [int]$state.automaticRecoveries } else { 0 }
    recoveredFromCycle = $state.recoveredFromCycle
    recoveryState = if ($Action -eq 'Start') { 'Queued' } elseif ($Action -eq 'Stop') { 'Stopped' } else { $state.recoveryState }
    heartbeatAt = $state.heartbeatAt
    lastFailureAt = $state.lastFailureAt
    nextRetryAt = if ($Action -eq 'Start') { $null } else { $state.nextRetryAt }
    updatedAt = [DateTimeOffset]::UtcNow.ToString('o')
}
if ($Action -ne 'Status') {
    $temporary = "$statePath.tmp"
    $payload | ConvertTo-Json | Set-Content -LiteralPath $temporary -Encoding UTF8
    Move-Item -LiteralPath $temporary -Destination $statePath -Force
}
if ($Action -eq 'Start') { Start-ScheduledTask -TaskName 'BOECL Autonomous Improvement' -ErrorAction SilentlyContinue }
[pscustomobject]$payload

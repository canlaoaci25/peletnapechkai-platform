function ConvertTo-BoeclUtcDate {
    [CmdletBinding()]
    param([AllowNull()][string]$Value)

    $parsed = [DateTimeOffset]::MinValue
    if ([string]::IsNullOrWhiteSpace($Value) -or -not [DateTimeOffset]::TryParse($Value, [Globalization.CultureInfo]::InvariantCulture, [Globalization.DateTimeStyles]::RoundtripKind, [ref]$parsed)) {
        return $null
    }
    return $parsed.ToUniversalTime()
}

function Get-BoeclWatchdogDecision {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][bool]$Enabled,
        [Parameter(Mandatory)][string]$TaskState,
        [AllowNull()][string]$CurrentStatus,
        [AllowNull()][string]$Heartbeat,
        [AllowNull()][string]$NextRetryAt,
        [DateTimeOffset]$Now = [DateTimeOffset]::UtcNow,
        [ValidateRange(1, 120)][int]$StaleAfterMinutes = 10
    )

    if (-not $Enabled) { return 'Disabled' }
    $deadline = ConvertTo-BoeclUtcDate -Value $NextRetryAt
    if ($null -ne $deadline -and $deadline -gt $Now) { return 'Backoff' }

    $heartbeatAt = ConvertTo-BoeclUtcDate -Value $Heartbeat
    $heartbeatStale = $null -eq $heartbeatAt -or ($Now - $heartbeatAt).TotalMinutes -ge $StaleAfterMinutes
    if ($CurrentStatus -eq 'Running' -and $heartbeatStale) { return 'RecoverStaleRun' }
    if ($TaskState -eq 'Disabled') { return 'EnableAndStart' }
    if ($TaskState -notin @('Running','Queued')) { return 'Start' }
    return 'Healthy'
}

function Test-BoeclRecoveryBudget {
    [CmdletBinding()]
    param(
        [AllowEmptyCollection()][object[]]$Recoveries = @(),
        [DateTimeOffset]$Now = [DateTimeOffset]::UtcNow,
        [ValidateRange(1, 20)][int]$MaximumRecoveries = 3,
        [ValidateRange(1, 1440)][int]$WindowMinutes = 30
    )

    $windowStart = $Now.AddMinutes(-$WindowMinutes)
    $recent = @($Recoveries | ForEach-Object { ConvertTo-BoeclUtcDate -Value ([string]$_) } | Where-Object { $null -ne $_ -and $_ -ge $windowStart })
    return [pscustomobject]@{ Allowed = $recent.Count -lt $MaximumRecoveries; Recent = $recent }
}

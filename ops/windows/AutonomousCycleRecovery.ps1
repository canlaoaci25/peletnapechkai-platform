function Get-BoeclRetryDelayMinutes {
    [CmdletBinding()]
    param([Parameter(Mandatory)][ValidateRange(1, 1000)][int]$ConsecutiveFailures)
    return [Math]::Min(60, [Math]::Pow(2, [Math]::Min($ConsecutiveFailures - 1, 6)))
}

function Test-BoeclUtcDeadlinePending {
    [CmdletBinding()]
    param([AllowNull()][string]$Deadline, [DateTimeOffset]$Now = [DateTimeOffset]::UtcNow)
    $parsed = [DateTimeOffset]::MinValue
    return -not [string]::IsNullOrWhiteSpace($Deadline) -and [DateTimeOffset]::TryParse($Deadline, [Globalization.CultureInfo]::InvariantCulture, [Globalization.DateTimeStyles]::RoundtripKind, [ref]$parsed) -and $parsed -gt $Now
}

function Test-BoeclHeartbeatStale {
    [CmdletBinding()]
    param([AllowNull()][string]$Heartbeat, [DateTimeOffset]$Now = [DateTimeOffset]::UtcNow, [int]$StaleAfterMinutes = 10)
    $parsed = [DateTimeOffset]::MinValue
    if ([string]::IsNullOrWhiteSpace($Heartbeat) -or -not [DateTimeOffset]::TryParse($Heartbeat, [Globalization.CultureInfo]::InvariantCulture, [Globalization.DateTimeStyles]::RoundtripKind, [ref]$parsed)) { return $true }
    return ($Now - $parsed).TotalMinutes -ge $StaleAfterMinutes
}

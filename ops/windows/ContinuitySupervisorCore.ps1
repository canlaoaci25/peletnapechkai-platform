Set-StrictMode -Version Latest

function Get-BoeclContinuityDecision {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][ValidateSet('Service','Task','Site','AppPool')][string]$Kind,
        [Parameter(Mandatory)][string]$State,
        [bool]$Enabled = $true,
        [Nullable[DateTimeOffset]]$LastRunAt,
        [Nullable[DateTimeOffset]]$NextRunAt,
        [long]$LastResult = 0,
        [ValidateRange(1,20160)][int]$MaximumSilenceMinutes = 10,
        [DateTimeOffset]$Now = [DateTimeOffset]::UtcNow
    )

    if ($Kind -in @('Service','Site','AppPool')) {
        if ($State -in @('Running','Started')) { return 'Healthy' }
        return 'Start'
    }
    if (-not $Enabled) { return 'EnableAndStart' }
    if ($State -in @('Running','Queued')) { return 'Healthy' }
    if ($null -eq $LastRunAt) { return 'Start' }
    if (($Now - [DateTimeOffset]$LastRunAt).TotalMinutes -gt $MaximumSilenceMinutes) { return 'Start' }
    if ($LastResult -ne 0 -and ($Now - [DateTimeOffset]$LastRunAt).TotalMinutes -ge 2) { return 'Start' }
    if ($null -ne $NextRunAt -and [DateTimeOffset]$NextRunAt -lt $Now.AddMinutes(-2)) { return 'Start' }
    'Healthy'
}

function Test-BoeclContinuityRecoveryBudget {
    [CmdletBinding()]
    param([object[]]$Actions=@(),[string]$Component,[DateTimeOffset]$Now=[DateTimeOffset]::UtcNow,[int]$Limit=3,[int]$WindowMinutes=30)
    $recent=@(foreach($action in $Actions){
        $parsed=[DateTimeOffset]::MinValue
        if($action.component -eq $Component -and [DateTimeOffset]::TryParse([string]$action.at,[ref]$parsed) -and ($Now-$parsed).TotalMinutes -le $WindowMinutes){$action}
    })
    [pscustomobject]@{Allowed=$recent.Count -lt $Limit;Recent=$recent}
}

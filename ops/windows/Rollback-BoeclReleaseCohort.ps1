[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$CohortId,
    [string]$WebRollbackPath = '',
    [string]$ApiRollbackPath = '',
    [string]$Root = 'C:\inetpub\peletnapechkai',
    [string]$WebService = 'PeletnapechkaiWeb',
    [string]$ApiPool = 'PeletnapechkaiApiPool',
    [switch]$SkipHealthCheck
)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'DeploymentJournal.ps1')
if ($CohortId -notmatch '^[a-zA-Z0-9-]{1,64}$') { throw 'Cohort id contains unsupported characters.' }
$resolvedRoot = [IO.Path]::GetFullPath($Root).TrimEnd([IO.Path]::DirectorySeparatorChar)
function Assert-RollbackPath([string]$Path, [string]$Prefix) {
    if ([string]::IsNullOrWhiteSpace($Path)) { return $null }
    $resolved = [IO.Path]::GetFullPath($Path)
    if (-not $resolved.StartsWith($resolvedRoot + [IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase) -or
        -not ([IO.Path]::GetFileName($resolved)).StartsWith($Prefix,[StringComparison]::OrdinalIgnoreCase) -or
        -not (Test-Path -LiteralPath $resolved -PathType Container)) { throw "Invalid or missing cohort rollback artifact: $Path" }
    return $resolved
}
$webRollback = Assert-RollbackPath $WebRollbackPath '.web-rollback-'
$apiRollback = Assert-RollbackPath $ApiRollbackPath '.api-rollback-'
if (-not $webRollback -and -not $apiRollback) { throw 'At least one rollback artifact is required.' }
$startedAt = [datetimeoffset]::UtcNow
try {
    Import-Module WebAdministration
    if ($webRollback) { Stop-Service -Name $WebService -Force }
    if ($apiRollback -and (Get-WebAppPoolState $ApiPool).Value -eq 'Started') { Stop-WebAppPool $ApiPool }
    foreach ($item in @(
        @{ Active=(Join-Path $resolvedRoot 'web'); Rollback=$webRollback; FailedPrefix='.web-failed-' },
        @{ Active=(Join-Path $resolvedRoot 'api'); Rollback=$apiRollback; FailedPrefix='.api-failed-' }
    )) {
        if (-not $item.Rollback) { continue }
        if (-not (Test-Path -LiteralPath $item.Active -PathType Container)) { throw "Active release is missing: $($item.Active)" }
        $failed = Join-Path $resolvedRoot ($item.FailedPrefix + $CohortId)
        if (Test-Path -LiteralPath $failed) { throw "Rollback quarantine already exists: $failed" }
        Move-Item -LiteralPath $item.Active -Destination $failed
        Move-Item -LiteralPath $item.Rollback -Destination $item.Active
    }
    if ($apiRollback) { Start-WebAppPool $ApiPool }
    if ($webRollback) { Start-Service -Name $WebService }
    if (-not $SkipHealthCheck) {
        & (Join-Path $PSScriptRoot 'Invoke-ProductionHealthCheck.ps1') | Out-Null
        if ($LASTEXITCODE -ne 0) { throw 'Production cohort rollback health gate failed.' }
    }
    foreach ($component in @($(if ($webRollback) {'Web'}),$(if ($apiRollback) {'Api'}))) {
        Write-BoeclDeploymentJournal -Environment Production -Component $component -Status RolledBack -DeploymentId $CohortId -StartedAt $startedAt -Message 'Release cohort rollback restored the previous healthy component.'
    }
    [pscustomobject]@{ CohortId=$CohortId; WebRolledBack=[bool]$webRollback; ApiRolledBack=[bool]$apiRollback; Healthy=(-not $SkipHealthCheck) }
} catch {
    foreach ($component in @($(if ($webRollback) {'Web'}),$(if ($apiRollback) {'Api'}))) {
        Write-BoeclDeploymentJournal -Environment Production -Component $component -Status RollbackFailed -DeploymentId $CohortId -StartedAt $startedAt -Message $_.Exception.Message
    }
    throw
}

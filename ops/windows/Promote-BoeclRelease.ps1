[CmdletBinding()]
param(
    [string]$RepositoryPath = (Join-Path $PSScriptRoot '..\..'),
    [switch]$SkipStaging
)
$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath($RepositoryPath)
$mutex = [Threading.Mutex]::new($false, 'Global\BoeclReleasePromotion')
$locked = $false
try {
    $locked = $mutex.WaitOne(0)
    if (-not $locked) { throw 'Another BOECL release promotion is already running.' }
    if ($SkipStaging) { throw 'Production promotion requires the staging deployment and health gate.' }
    $commit = (& git.exe -C $repository rev-parse --short=12 HEAD 2>$null)
    if (-not $commit) { throw 'The release commit could not be resolved.' }
    $dirty = (& git.exe -C $repository status --porcelain)
    if ($dirty) { throw 'Release promotion requires a clean working tree.' }
    & (Join-Path $PSScriptRoot 'Test-WebReleaseBudget.ps1') -BuildRoot (Join-Path $repository 'apps\web')
    if ($LASTEXITCODE -ne 0) { throw 'Web release performance budget failed before staging deployment.' }

    if (-not $SkipStaging) {
        & (Join-Path $PSScriptRoot 'Deploy-AspNetApiRelease.ps1') -Environment Staging -RepositoryPath $repository
        if ($LASTEXITCODE -ne 0) { throw 'Staging API deployment failed.' }
        & (Join-Path $PSScriptRoot 'Deploy-NextWebRelease.ps1') -Environment Staging -BuildRoot (Join-Path $repository 'apps\web')
        if ($LASTEXITCODE -ne 0) { throw 'Staging web deployment failed.' }
        & (Join-Path $PSScriptRoot 'Invoke-StagingHealthCheck.ps1')
        if ($LASTEXITCODE -ne 0) { throw 'Staging promotion gate failed.' }
    }

    $cohortId = 'cohort-' + [guid]::NewGuid().ToString('N')
    $apiDeployment = & (Join-Path $PSScriptRoot 'Deploy-AspNetApiRelease.ps1') -Environment Production -RepositoryPath $repository -CohortId $cohortId
    if ($LASTEXITCODE -ne 0) { throw 'Production API deployment failed.' }
    try {
        $webDeployment = & (Join-Path $PSScriptRoot 'Deploy-NextWebRelease.ps1') -Environment Production -BuildRoot (Join-Path $repository 'apps\web') -CohortId $cohortId
        if ($LASTEXITCODE -ne 0) { throw 'Production web deployment failed.' }
        & (Join-Path $PSScriptRoot 'Invoke-ProductionHealthCheck.ps1')
        if ($LASTEXITCODE -ne 0) { throw 'Production promotion gate failed.' }
    } catch {
        $webRollback = if ($webDeployment -and (Test-Path -LiteralPath $webDeployment.Rollback)) { $webDeployment.Rollback } else { '' }
        & (Join-Path $PSScriptRoot 'Rollback-BoeclReleaseCohort.ps1') -CohortId $cohortId -WebRollbackPath $webRollback -ApiRollbackPath $apiDeployment.Rollback
        throw
    }
    [pscustomobject]@{ Commit=$commit; CohortId=$cohortId; Staging=(-not $SkipStaging); Production=$true; Healthy=$true }
}
finally {
    if ($locked) { $mutex.ReleaseMutex() }
    $mutex.Dispose()
}

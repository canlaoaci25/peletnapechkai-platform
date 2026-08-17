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
    $commit = (& git.exe -C $repository rev-parse --short=12 HEAD 2>$null)
    if (-not $commit) { throw 'The release commit could not be resolved.' }
    $dirty = (& git.exe -C $repository status --porcelain)
    if ($dirty) { throw 'Release promotion requires a clean working tree.' }

    if (-not $SkipStaging) {
        & (Join-Path $PSScriptRoot 'Deploy-AspNetApiRelease.ps1') -Environment Staging -RepositoryPath $repository
        if ($LASTEXITCODE -ne 0) { throw 'Staging API deployment failed.' }
        & (Join-Path $PSScriptRoot 'Deploy-NextWebRelease.ps1') -Environment Staging -BuildRoot (Join-Path $repository 'apps\web')
        if ($LASTEXITCODE -ne 0) { throw 'Staging web deployment failed.' }
        & (Join-Path $PSScriptRoot 'Invoke-StagingHealthCheck.ps1')
        if ($LASTEXITCODE -ne 0) { throw 'Staging promotion gate failed.' }
    }

    & (Join-Path $PSScriptRoot 'Deploy-AspNetApiRelease.ps1') -Environment Production -RepositoryPath $repository
    if ($LASTEXITCODE -ne 0) { throw 'Production API deployment failed.' }
    & (Join-Path $PSScriptRoot 'Deploy-NextWebRelease.ps1') -Environment Production -BuildRoot (Join-Path $repository 'apps\web')
    if ($LASTEXITCODE -ne 0) { throw 'Production web deployment failed. The web deploy restored its previous healthy release.' }
    & (Join-Path $PSScriptRoot 'Invoke-ProductionHealthCheck.ps1')
    if ($LASTEXITCODE -ne 0) { throw 'Production promotion gate failed.' }
    [pscustomobject]@{ Commit=$commit; Staging=(-not $SkipStaging); Production=$true; Healthy=$true }
}
finally {
    if ($locked) { $mutex.ReleaseMutex() }
    $mutex.Dispose()
}

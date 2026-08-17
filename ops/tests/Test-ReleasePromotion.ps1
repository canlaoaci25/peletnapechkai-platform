[CmdletBinding()] param()
$ErrorActionPreference = 'Stop'
$script = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\windows\Promote-BoeclRelease.ps1')
if ($script -notmatch 'Global\\BoeclReleasePromotion' -or $script -notmatch 'WaitOne\(0\)') { throw 'Release promotion must reject concurrent runs.' }
if ($script -notmatch 'status --porcelain' -or $script -notmatch 'clean working tree') { throw 'Release promotion must reject uncommitted input.' }
$stagingApi = $script.IndexOf("-Environment Staging -RepositoryPath")
$stagingWeb = $script.IndexOf("-Environment Staging -BuildRoot")
$productionApi = $script.IndexOf("-Environment Production -RepositoryPath")
$productionWeb = $script.IndexOf("-Environment Production -BuildRoot")
if ($stagingApi -lt 0 -or $stagingWeb -le $stagingApi -or $productionApi -le $stagingWeb -or $productionWeb -le $productionApi) { throw 'Release promotion order is unsafe.' }
if ($script -notmatch 'Invoke-StagingHealthCheck.ps1' -or $script -notmatch 'Invoke-ProductionHealthCheck.ps1') { throw 'Release promotion lacks final environment gates.' }
Write-Host 'Release promotion regression tests passed.'

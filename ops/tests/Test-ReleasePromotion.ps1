[CmdletBinding()] param()
$ErrorActionPreference = 'Stop'
$script = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\windows\Promote-BoeclRelease.ps1')
if ($script -notmatch 'Global\\BoeclReleasePromotion' -or $script -notmatch 'WaitOne\(0\)') { throw 'Release promotion must reject concurrent runs.' }
if ($script -notmatch 'status --porcelain' -or $script -notmatch 'clean working tree') { throw 'Release promotion must reject uncommitted input.' }
$budget = $script.IndexOf('Test-WebReleaseBudget.ps1')
$stagingApi = $script.IndexOf("-Environment Staging -RepositoryPath")
$stagingWeb = $script.IndexOf("-Environment Staging -BuildRoot")
$productionApi = $script.IndexOf("-Environment Production -RepositoryPath")
$productionWeb = $script.IndexOf("-Environment Production -BuildRoot")
if ($budget -lt 0 -or $stagingApi -le $budget -or $stagingWeb -le $stagingApi -or $productionApi -le $stagingWeb -or $productionWeb -le $productionApi) { throw 'Release promotion order is unsafe.' }
if ($script -notmatch 'Invoke-StagingHealthCheck.ps1' -or $script -notmatch 'Invoke-ProductionHealthCheck.ps1') { throw 'Release promotion lacks final environment gates.' }
if (-not $script.Contains("if (`$SkipStaging) { throw 'Production promotion requires the staging deployment and health gate.' }")) { throw 'Release promotion must fail closed when staging is skipped.' }
if ($script -notmatch "\$cohortId = 'cohort-'" -or ([regex]::Matches($script,'-CohortId \$cohortId').Count -lt 2)) { throw 'Web and API must share one release cohort id.' }
if ($script -notmatch 'Rollback-BoeclReleaseCohort.ps1' -or $script.IndexOf('Rollback-BoeclReleaseCohort.ps1') -le $productionWeb) { throw 'Production failures must invoke coordinated cohort rollback.' }
$rollbackScript = Join-Path $PSScriptRoot '..\windows\Rollback-BoeclReleaseCohort.ps1'
$rejectedUnsafePath = $false
try { & $rollbackScript -CohortId 'fixture' -ApiRollbackPath 'C:\Windows' -SkipHealthCheck } catch { $rejectedUnsafePath = $_.Exception.Message -match 'Invalid or missing cohort rollback artifact' }
if (-not $rejectedUnsafePath) { throw 'Cohort rollback did not reject an out-of-root artifact before mutation.' }
Write-Host 'Release promotion regression tests passed.'

[CmdletBinding()] param()
$ErrorActionPreference = 'Stop'
$script = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\windows\Promote-BoeclRelease.ps1')
if ($script -notmatch 'Global\\BoeclReleasePromotion' -or $script -notmatch 'WaitOne\(0\)') { throw 'Release promotion must reject concurrent runs.' }
if ($script -notmatch 'status --porcelain' -or $script -notmatch 'clean working tree') { throw 'Release promotion must reject uncommitted input.' }
$budget = $script.IndexOf('Test-WebReleaseBudget.ps1')
$stagingBackup = $script.IndexOf("Backup-PostgreSql.ps1') -Environment Staging")
$stagingRestore = $script.IndexOf('Test-PostgreSqlRestore.ps1')
$stagingMigration = $script.IndexOf("Update-BoeclDatabase.ps1') -Environment Staging")
$stagingApi = $script.IndexOf("Deploy-AspNetApiRelease.ps1') -Environment Staging -RepositoryPath")
$stagingWeb = $script.IndexOf("-Environment Staging -BuildRoot")
$productionPreflight = $script.IndexOf('Test-ProductionHealth.ps1')
$productionBackup = $script.IndexOf("Backup-PostgreSql.ps1') -Environment Production")
$productionRestore = $script.IndexOf('Test-PostgreSqlRestore.ps1', $stagingRestore + 1)
$productionMigration = $script.IndexOf("Update-BoeclDatabase.ps1') -Environment Production")
$productionApi = $script.IndexOf("Deploy-AspNetApiRelease.ps1') -Environment Production -RepositoryPath")
$productionWeb = $script.IndexOf("-Environment Production -BuildRoot")
if ($budget -lt 0 -or $stagingBackup -le $budget -or $stagingRestore -le $stagingBackup -or $stagingMigration -le $stagingRestore -or
    $stagingApi -le $stagingMigration -or $stagingWeb -le $stagingApi -or $productionPreflight -le $stagingWeb -or
    $productionBackup -le $productionPreflight -or
    $productionRestore -le $productionBackup -or $productionMigration -le $productionRestore -or
    $productionApi -le $productionMigration -or $productionWeb -le $productionApi) { throw 'Release promotion order is unsafe.' }
if ([regex]::Matches($script, '-BackupPath \$[a-z]+Backup\.Backup').Count -lt 2) { throw 'Each promotion database backup must be restore-tested by exact path.' }
if ($script -notmatch 'Production preflight health gate failed before database or application mutation') { throw 'Production promotion lacks a non-mutating preflight health gate.' }
if ($script -notmatch 'Invoke-StagingHealthCheck.ps1' -or $script -notmatch 'Invoke-ProductionHealthCheck.ps1') { throw 'Release promotion lacks final environment gates.' }
if (-not $script.Contains("if (`$SkipStaging) { throw 'Production promotion requires the staging deployment and health gate.' }")) { throw 'Release promotion must fail closed when staging is skipped.' }
if ($script -notmatch "\$cohortId = 'cohort-'" -or ([regex]::Matches($script,'-CohortId \$cohortId').Count -lt 2)) { throw 'Web and API must share one release cohort id.' }
if ($script -notmatch 'Rollback-BoeclReleaseCohort.ps1' -or $script.IndexOf('Rollback-BoeclReleaseCohort.ps1') -le $productionWeb) { throw 'Production failures must invoke coordinated cohort rollback.' }
$rollbackScript = Join-Path $PSScriptRoot '..\windows\Rollback-BoeclReleaseCohort.ps1'
$rejectedUnsafePath = $false
try { & $rollbackScript -CohortId 'fixture' -ApiRollbackPath 'C:\Windows' -SkipHealthCheck } catch { $rejectedUnsafePath = $_.Exception.Message -match 'Invalid or missing cohort rollback artifact' }
if (-not $rejectedUnsafePath) { throw 'Cohort rollback did not reject an out-of-root artifact before mutation.' }
Write-Host 'Release promotion regression tests passed.'

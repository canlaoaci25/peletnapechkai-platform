$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$cycle = Get-Content -Raw -LiteralPath (Join-Path $root 'ops\windows\Invoke-BoeclAutonomousCycle.ps1')
$backup = Get-Content -Raw -LiteralPath (Join-Path $root 'ops\windows\Backup-PostgreSql.ps1')
$restore = Get-Content -Raw -LiteralPath (Join-Path $root 'ops\windows\Test-PostgreSqlRestore.ps1')

foreach ($environment in @('Staging','Production')) {
    if ($cycle -notmatch "Backup-PostgreSql\.ps1'\) -Environment $environment") { throw "$environment backup is not explicitly targeted." }
}
if ([regex]::Matches($cycle, 'Test-PostgreSqlRestore\.ps1').Count -lt 2) { throw 'Both migration environments must restore-test their exact backup.' }
$stagingHealth = $cycle.IndexOf('Invoke-StagingHealthCheck.ps1')
$productionBackup = $cycle.IndexOf("Backup-PostgreSql.ps1') -Environment Production")
$productionApi = $cycle.IndexOf("Deploy-AspNetApiRelease.ps1') -Environment Production")
if ($stagingHealth -lt 0 -or $productionBackup -le $stagingHealth -or $productionApi -le $stagingHealth) { throw 'Production must remain blocked until the complete staging cohort passes health checks.' }
if ($cycle -notmatch 'Invoke-ProductionHealthCheck\.ps1') { throw 'Autonomous promotion must close with a production health gate.' }
if (-not $cycle.Contains("`$productionCohortId = 'cohort-'") -or ([regex]::Matches($cycle,'-CohortId \$productionCohortId').Count -lt 2)) { throw 'Autonomous Web and API production deployments must share one release cohort id.' }
$productionWeb = $cycle.IndexOf("Deploy-NextWebRelease.ps1') -Environment Production")
$cohortRollback = $cycle.IndexOf('Rollback-BoeclReleaseCohort.ps1')
if ($productionWeb -lt 0 -or $cohortRollback -le $productionWeb) { throw 'Autonomous production failures must invoke coordinated cohort rollback after deployment.' }
if ($cycle -notmatch "ContainsKey\('WebRollbackPath'\)" -or $cycle -notmatch "ContainsKey\('ApiRollbackPath'\)") { throw 'Autonomous rollback must require a deployment artifact before mutation.' }
if ($backup -notmatch "ValidateSet\('Development','Staging','Production'\)" -or $backup -notmatch 'ConnectionStrings__Database') { throw 'Backup script cannot resolve an explicit IIS environment target.' }
if ($restore -notmatch '\[string\]\$BackupPath' -or $restore -notmatch 'Get-Item -LiteralPath \$BackupPath') { throw 'Restore test cannot pin the backup produced by the current promotion.' }
if ($restore -notmatch '\[int\]\$localeCount -lt 3' -or $restore -notmatch '\[int\]\$localeCount -gt 4') { throw 'Restore validation must accept only the legacy or current supported-locale baseline.' }
if ($cycle -notmatch 'Update-BoeclDatabase\.ps1') { throw 'Promotion must run the locale-parity migration after restore validation.' }
Write-Host 'Database backup promotion regression tests passed.'

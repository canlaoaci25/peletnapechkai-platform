$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$cycle = Get-Content -Raw -LiteralPath (Join-Path $root 'ops\windows\Invoke-BoeclAutonomousCycle.ps1')
$backup = Get-Content -Raw -LiteralPath (Join-Path $root 'ops\windows\Backup-PostgreSql.ps1')
$restore = Get-Content -Raw -LiteralPath (Join-Path $root 'ops\windows\Test-PostgreSqlRestore.ps1')

foreach ($environment in @('Staging','Production')) {
    if ($cycle -notmatch "Backup-PostgreSql\.ps1'\) -Environment $environment") { throw "$environment backup is not explicitly targeted." }
}
if ([regex]::Matches($cycle, 'Test-PostgreSqlRestore\.ps1').Count -lt 2) { throw 'Both migration environments must restore-test their exact backup.' }
if ($backup -notmatch "ValidateSet\('Development','Staging','Production'\)" -or $backup -notmatch 'ConnectionStrings__Database') { throw 'Backup script cannot resolve an explicit IIS environment target.' }
if ($restore -notmatch '\[string\]\$BackupPath' -or $restore -notmatch 'Get-Item -LiteralPath \$BackupPath') { throw 'Restore test cannot pin the backup produced by the current promotion.' }
if ($restore -notmatch '\[int\]\$localeCount -ne 4') { throw 'Restore validation must preserve all four supported locales.' }
Write-Host 'Database backup promotion regression tests passed.'

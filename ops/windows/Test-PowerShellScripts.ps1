[CmdletBinding()]
param([string]$RepositoryPath = '')

$ErrorActionPreference = 'Stop'
if (-not $RepositoryPath) { $RepositoryPath = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path }
$failures = [Collections.Generic.List[string]]::new()
Get-ChildItem -LiteralPath (Join-Path $RepositoryPath 'ops') -Filter '*.ps1' -File -Recurse | ForEach-Object {
    $tokens = $null
    $errors = $null
    [Management.Automation.Language.Parser]::ParseFile($_.FullName, [ref]$tokens, [ref]$errors) | Out-Null
    foreach ($error in $errors) { $failures.Add("$($_.FullName): $($error.Message)") }
}
if ($failures.Count -gt 0) { throw ($failures -join [Environment]::NewLine) }
Write-Host 'Tüm operasyon PowerShell betikleri başarıyla ayrıştırıldı.'

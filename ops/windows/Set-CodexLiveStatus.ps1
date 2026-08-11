[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Task,
    [Parameter(Mandatory)][string]$Phase,
    [ValidateSet('Working','Completed','Failed','Paused')][string]$Status = 'Working',
    [string[]]$Steps = @(),
    [int]$CurrentStep = 0,
    [string]$LastAction = '',
    [string]$Commit = '',
    [string]$Root = 'C:\ProgramData\Peletnapechkai\LiveDevelopment'
)
$ErrorActionPreference = 'Stop'
New-Item -ItemType Directory -Path $Root -Force | Out-Null
$path = Join-Path $Root 'status.json'
$startedAt = (Get-Date).ToUniversalTime().ToString('o')
if (Test-Path -LiteralPath $path) {
    try { $existing = Get-Content -Raw -LiteralPath $path | ConvertFrom-Json; if ($existing.task -eq $Task -and $existing.startedAt) { $startedAt = $existing.startedAt } } catch { }
}
$payload = [ordered]@{ task=$Task; phase=$Phase; status=$Status; steps=$Steps; currentStep=$CurrentStep; lastAction=$LastAction; commit=$Commit; startedAt=$startedAt; updatedAt=(Get-Date).ToUniversalTime().ToString('o'); machine=$env:COMPUTERNAME }
$temporary = Join-Path $Root ("status-{0}.tmp" -f [guid]::NewGuid().ToString('N'))
$payload | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $temporary -Encoding utf8
Move-Item -LiteralPath $temporary -Destination $path -Force

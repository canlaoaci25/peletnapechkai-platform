[CmdletBinding()]
param([string]$RepositoryPath = 'C:\Users\Administrator\Desktop\peletnapechkai-platform')

$ErrorActionPreference = 'Stop'
$script = Join-Path ([IO.Path]::GetFullPath($RepositoryPath)) 'ops\windows\Invoke-BoeclAutonomousCycle.ps1'
if (-not (Test-Path -LiteralPath $script)) { throw "Orkestratör bulunamadı: $script" }
$encodedPath = $script.Replace("'", "''")
$action = New-ScheduledTaskAction -Execute 'powershell.exe' -Argument "-NoProfile -ExecutionPolicy Bypass -Command `"& ([scriptblock]::Create((Get-Content -Raw -Encoding UTF8 '$encodedPath')))`""
$trigger = New-ScheduledTaskTrigger -Once -At (Get-Date).AddMinutes(1) -RepetitionInterval (New-TimeSpan -Minutes 1)
$settings = New-ScheduledTaskSettingsSet -MultipleInstances IgnoreNew -StartWhenAvailable -ExecutionTimeLimit (New-TimeSpan -Hours 6)
$principal = New-ScheduledTaskPrincipal -UserId 'Administrator' -LogonType Interactive -RunLevel Highest
Register-ScheduledTask -TaskName 'BOECL Autonomous Improvement' -Action $action -Trigger $trigger -Settings $settings -Principal $principal -Description 'BOECL kod, API, içerik, SEO, çeviri ve tasarım öz-geliştirme çevrimi.' -Force | Out-Null
& (Join-Path ([IO.Path]::GetFullPath($RepositoryPath)) 'ops\windows\Set-BoeclAutonomousMode.ps1') -Action Stop | Out-Null
Get-ScheduledTask -TaskName 'BOECL Autonomous Improvement'

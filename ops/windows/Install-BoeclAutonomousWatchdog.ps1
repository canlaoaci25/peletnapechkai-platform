[CmdletBinding()]
param([string]$RepositoryPath = 'C:\Users\Administrator\Desktop\peletnapechkai-platform')

$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath($RepositoryPath)
$script = Join-Path $repository 'ops\windows\Invoke-BoeclAutonomousWatchdog.ps1'
$core = Join-Path $repository 'ops\windows\AutonomousWatchdogCore.ps1'
if (-not (Test-Path -LiteralPath $script -PathType Leaf) -or -not (Test-Path -LiteralPath $core -PathType Leaf)) { throw 'Watchdog betikleri bulunamadi.' }

$action = New-ScheduledTaskAction -Execute 'C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe' `
    -Argument "-NoProfile -NonInteractive -ExecutionPolicy Bypass -File `"$script`"" -WorkingDirectory $repository
$startup = New-ScheduledTaskTrigger -AtStartup
$interval = New-ScheduledTaskTrigger -Once -At (Get-Date).AddMinutes(1) -RepetitionInterval (New-TimeSpan -Minutes 2)
$settings = New-ScheduledTaskSettingsSet -MultipleInstances IgnoreNew -StartWhenAvailable -ExecutionTimeLimit (New-TimeSpan -Minutes 2) `
    -RestartCount 2 -RestartInterval (New-TimeSpan -Minutes 1)
$principal = New-ScheduledTaskPrincipal -UserId 'SYSTEM' -LogonType ServiceAccount -RunLevel Highest
Register-ScheduledTask -TaskName 'BOECL Autonomous Watchdog' -Action $action -Trigger @($startup,$interval) -Settings $settings `
    -Principal $principal -Description 'BOECL otonom gelistirme gorevini heartbeat ve surec durumuyla bagimsiz denetler.' -Force | Out-Null
Start-ScheduledTask -TaskName 'BOECL Autonomous Watchdog'
Get-ScheduledTask -TaskName 'BOECL Autonomous Watchdog'

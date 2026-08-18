[CmdletBinding()]
param([string]$RepositoryPath='C:\Users\Administrator\Desktop\peletnapechkai-platform')
$ErrorActionPreference='Stop'
$repository=[IO.Path]::GetFullPath($RepositoryPath)
$script=Join-Path $repository 'ops\windows\Invoke-BoeclContinuitySupervisor.ps1'
if(-not(Test-Path -LiteralPath $script -PathType Leaf)){throw "Supervisor betigi bulunamadi: $script"}
$action=New-ScheduledTaskAction -Execute 'C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe' -Argument "-NoProfile -NonInteractive -ExecutionPolicy Bypass -File `"$script`"" -WorkingDirectory $repository
$startup=New-ScheduledTaskTrigger -AtStartup
$interval=New-ScheduledTaskTrigger -Once -At (Get-Date).AddMinutes(1) -RepetitionInterval (New-TimeSpan -Minutes 2)
$settings=New-ScheduledTaskSettingsSet -MultipleInstances IgnoreNew -StartWhenAvailable -ExecutionTimeLimit (New-TimeSpan -Minutes 5)
$principal=New-ScheduledTaskPrincipal -UserId 'SYSTEM' -LogonType ServiceAccount -RunLevel Highest
Register-ScheduledTask -TaskName 'BOECL Continuity Supervisor' -Action $action -Trigger @($startup,$interval) -Settings $settings -Principal $principal -Description 'BOECL servis, IIS, worker, saglik, sitemap, yedek ve watchdog sureklilik koruyucusu.' -Force|Out-Null
Start-ScheduledTask -TaskName 'BOECL Continuity Supervisor'
Get-ScheduledTask -TaskName 'BOECL Continuity Supervisor'

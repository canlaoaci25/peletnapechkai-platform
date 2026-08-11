[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string]$WorkerToken,
    [string]$RepositoryPath = 'C:\Users\Administrator\Desktop\peletnapechkai-platform',
    [string]$CodexPath = 'C:\Users\Administrator\AppData\Local\Programs\OpenAI\Codex\bin\codex.exe',
    [string]$CodexHome = 'C:\Users\Administrator\.codex',
    [string]$ApiUrl = 'http://127.0.0.1:5080',
    [string]$InstallRoot = 'C:\ProgramData\Peletnapechkai\AutomationWorker'
)

$ErrorActionPreference = 'Stop'
$secretRoot = 'C:\ProgramData\Peletnapechkai\Secrets'
$scriptPath = Join-Path $InstallRoot 'Invoke-BoeclCodexWorker.ps1'
$configPath = Join-Path $secretRoot 'automation-worker.json'
New-Item -ItemType Directory -Path $InstallRoot, $secretRoot -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Invoke-BoeclCodexWorker.ps1') -Destination $scriptPath -Force

@{
    workerToken = $WorkerToken
    repositoryPath = $RepositoryPath
    codexPath = $CodexPath
    codexHome = $CodexHome
    apiUrl = $ApiUrl
} | ConvertTo-Json | Set-Content -LiteralPath $configPath -Encoding utf8

& icacls.exe $secretRoot /inheritance:r /grant:r 'SYSTEM:(OI)(CI)F' 'Administrators:(OI)(CI)F' | Out-Null
& icacls.exe $InstallRoot /inheritance:r /grant:r 'SYSTEM:(OI)(CI)RX' 'Administrators:(OI)(CI)F' | Out-Null

$arguments = "-NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File `"$scriptPath`" -ConfigPath `"$configPath`""
$action = New-ScheduledTaskAction -Execute 'C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe' -Argument $arguments -WorkingDirectory $RepositoryPath
$startup = New-ScheduledTaskTrigger -AtStartup
$minute = New-ScheduledTaskTrigger -Once -At (Get-Date).AddMinutes(1) -RepetitionInterval (New-TimeSpan -Minutes 1)
$settings = New-ScheduledTaskSettingsSet -MultipleInstances IgnoreNew -StartWhenAvailable -ExecutionTimeLimit (New-TimeSpan -Hours 6) -RestartCount 3 -RestartInterval (New-TimeSpan -Minutes 1)
$workerUser = [Security.Principal.WindowsIdentity]::GetCurrent().Name
$principal = New-ScheduledTaskPrincipal -UserId $workerUser -LogonType Interactive -RunLevel Highest
Register-ScheduledTask -TaskName 'BOECL Codex Automation Worker' -Action $action -Trigger @($startup, $minute) -Settings $settings -Principal $principal -Force | Out-Null

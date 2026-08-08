[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$BackupPath,
    [string]$ConfigPath = 'C:\ProgramData\Peletnapechkai\Secrets\offsite-backup.json'
)

$ErrorActionPreference = 'Stop'
$logPath = 'C:\ProgramData\Peletnapechkai\Logs\offsite-backup.log'
New-Item -ItemType Directory -Path (Split-Path $logPath) -Force | Out-Null

if (-not (Test-Path -LiteralPath $ConfigPath)) { return }
$config = Get-Content -LiteralPath $ConfigPath -Raw | ConvertFrom-Json
if (-not $config.Enabled) { return }

foreach ($property in 'HostName', 'UserName', 'RemotePath', 'PrivateKeyPath', 'KnownHostsPath') {
    if ([string]::IsNullOrWhiteSpace($config.$property)) { throw "Missing SFTP setting: $property" }
}
if (-not (Test-Path -LiteralPath $BackupPath)) { throw "Backup does not exist: $BackupPath" }
$checksumPath = "$BackupPath.sha256"
if (-not (Test-Path -LiteralPath $checksumPath)) { throw "Checksum does not exist: $checksumPath" }
if (-not (Test-Path -LiteralPath $config.PrivateKeyPath)) { throw 'SFTP private key does not exist.' }
if (-not (Test-Path -LiteralPath $config.KnownHostsPath)) { throw 'SFTP known-hosts file does not exist.' }

$port = if ($config.Port) { [int]$config.Port } else { 22 }
$dateDirectory = Get-Date -Format 'yyyy-MM-dd'
$remoteRoot = $config.RemotePath.TrimEnd('/')
$remoteDay = "$remoteRoot/$dateDirectory"
$batchPath = Join-Path $env:TEMP "peletnapechkai-sftp-$([guid]::NewGuid().ToString('N')).txt"

try {
    @(
        "-mkdir $remoteRoot"
        "-mkdir $remoteDay"
        "put `"$BackupPath`" `"$remoteDay/$([IO.Path]::GetFileName($BackupPath))`""
        "put `"$checksumPath`" `"$remoteDay/$([IO.Path]::GetFileName($checksumPath))`""
    ) | Set-Content -LiteralPath $batchPath -Encoding ascii

    & "$env:SystemRoot\System32\OpenSSH\sftp.exe" -b $batchPath -P $port `
        -i $config.PrivateKeyPath `
        -o BatchMode=yes `
        -o StrictHostKeyChecking=yes `
        -o "UserKnownHostsFile=$($config.KnownHostsPath)" `
        "$($config.UserName)@$($config.HostName)"
    if ($LASTEXITCODE -ne 0) { throw "SFTP upload failed with exit code $LASTEXITCODE." }

    "$(Get-Date -Format o) SUCCESS $($config.HostName) $remoteDay $([IO.Path]::GetFileName($BackupPath))" |
        Add-Content -LiteralPath $logPath
}
catch {
    "$(Get-Date -Format o) FAILURE $($_.Exception.Message)" | Add-Content -LiteralPath $logPath
    throw
}
finally {
    Remove-Item -LiteralPath $batchPath -Force -ErrorAction SilentlyContinue
}

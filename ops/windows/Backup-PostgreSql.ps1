[CmdletBinding()]
param(
    [string]$Database = 'peletnapechkai_dev',
    [string]$HostName = '127.0.0.1',
    [int]$Port = 5432,
    [string]$UserName = 'peletnapechkai_owner',
    [string]$BackupRoot = 'C:\ProgramData\Peletnapechkai\Backups\PostgreSQL',
    [string]$PasswordFile = 'C:\ProgramData\Peletnapechkai\Secrets\pgpass.conf',
    [int]$RetentionDays = 30,
    [string]$OffsiteConfigPath = 'C:\ProgramData\Peletnapechkai\Secrets\offsite-backup.json'
)

$ErrorActionPreference = 'Stop'
$postgresBin = 'C:\Program Files\PostgreSQL\18\bin'
$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$dailyDirectory = Join-Path $BackupRoot (Get-Date -Format 'yyyy-MM-dd')
$backupPath = Join-Path $dailyDirectory "$Database-$timestamp.dump"
$checksumPath = "$backupPath.sha256"
$logPath = 'C:\ProgramData\Peletnapechkai\Logs\postgresql-backup.log'

New-Item -ItemType Directory -Path $dailyDirectory -Force | Out-Null
New-Item -ItemType Directory -Path (Split-Path $logPath) -Force | Out-Null

try {
    $env:PGPASSFILE = $PasswordFile
    $dumpArguments = @(
        '--host', $HostName,
        '--port', $Port,
        '--username', $UserName,
        '--dbname', $Database,
        '--format', 'custom',
        '--compress', 'zstd:9',
        '--no-password',
        '--file', $backupPath
    )
    & "$postgresBin\pg_dump.exe" @dumpArguments

    if ($LASTEXITCODE -ne 0) {
        throw "pg_dump failed with exit code $LASTEXITCODE."
    }

    $hash = Get-FileHash -LiteralPath $backupPath -Algorithm SHA256
    "$($hash.Hash)  $([IO.Path]::GetFileName($backupPath))" | Set-Content -LiteralPath $checksumPath -Encoding ascii

    Get-ChildItem -LiteralPath $BackupRoot -Directory -ErrorAction SilentlyContinue |
        Where-Object LastWriteTime -lt (Get-Date).AddDays(-$RetentionDays) |
        Remove-Item -Recurse -Force

    "$(Get-Date -Format o) SUCCESS $backupPath $((Get-Item $backupPath).Length) bytes" |
        Add-Content -LiteralPath $logPath

    if (Test-Path -LiteralPath $OffsiteConfigPath) {
        & (Join-Path $PSScriptRoot 'Send-BackupSftp.ps1') -BackupPath $backupPath -ConfigPath $OffsiteConfigPath
    }
}
catch {
    "$(Get-Date -Format o) FAILURE $($_.Exception.Message)" | Add-Content -LiteralPath $logPath
    throw
}
finally {
    Remove-Item Env:PGPASSFILE -ErrorAction SilentlyContinue
}

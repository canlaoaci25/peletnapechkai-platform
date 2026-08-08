[CmdletBinding()]
param(
    [string]$BackupRoot = 'C:\ProgramData\Peletnapechkai\Backups\PostgreSQL',
    [string]$PasswordFile = 'C:\ProgramData\Peletnapechkai\Secrets\pgpass.conf',
    [string]$HostName = '127.0.0.1',
    [int]$Port = 5432,
    [string]$UserName = 'peletnapechkai_owner'
)

$ErrorActionPreference = 'Stop'
$postgresBin = 'C:\Program Files\PostgreSQL\18\bin'
$backup = Get-ChildItem -LiteralPath $BackupRoot -Filter '*.dump' -Recurse |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if (-not $backup) {
    throw 'No PostgreSQL backup is available for restore testing.'
}

$database = "peletnapechkai_restore_$((Get-Date).ToString('yyyyMMddHHmmss'))"
$env:PGPASSFILE = $PasswordFile

try {
    & "$postgresBin\createdb.exe" --host $HostName --port $Port --username $UserName --no-password $database
    if ($LASTEXITCODE -ne 0) { throw 'Could not create the restore-test database.' }

    $restoreArguments = @(
        '--host', $HostName,
        '--port', $Port,
        '--username', $UserName,
        '--dbname', $database,
        '--no-owner',
        '--no-privileges',
        '--exit-on-error',
        $backup.FullName
    )
    & "$postgresBin\pg_restore.exe" @restoreArguments
    if ($LASTEXITCODE -ne 0) { throw 'pg_restore failed.' }

    $migrationCount = 'SELECT COUNT(*) FROM "__EFMigrationsHistory";' |
        & "$postgresBin\psql.exe" --host $HostName --port $Port --username $UserName --dbname $database --tuples-only --no-align
    $localeCount = 'SELECT COUNT(*) FROM locales;' |
        & "$postgresBin\psql.exe" --host $HostName --port $Port --username $UserName --dbname $database --tuples-only --no-align

    if ([int]$migrationCount -lt 4 -or [int]$localeCount -lt 3) {
        throw "Restore validation failed: migrations=$migrationCount locales=$localeCount"
    }

    [pscustomobject]@{
        Backup = $backup.FullName
        Database = $database
        Migrations = [int]$migrationCount
        Locales = [int]$localeCount
        Result = 'Success'
    }
}
finally {
    & "$postgresBin\dropdb.exe" --host $HostName --port $Port --username $UserName --no-password --if-exists $database
    Remove-Item Env:PGPASSFILE -ErrorAction SilentlyContinue
}

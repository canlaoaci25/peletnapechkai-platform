# Production operations

Operational scripts live under `ops/windows`. They contain no credentials.

## PostgreSQL backups

The production server stores daily custom-format PostgreSQL backups under
`C:\ProgramData\Peletnapechkai\Backups\PostgreSQL`. The scheduled task runs as SYSTEM
and reads its PostgreSQL password from an ACL-restricted `pgpass.conf` outside the
repository. Backups include a SHA-256 checksum and are retained for 30 days on-server.

An on-server backup is not sufficient disaster recovery. Copy backups to a second,
independently protected location before launch.

### Optional off-server SFTP copy

The daily backup can upload both the dump and checksum to an SFTP server immediately
after creation. Copy `ops/windows/offsite-backup.example.json` to
`C:\ProgramData\Peletnapechkai\Secrets\offsite-backup.json`, configure it, and set
`Enabled` to `true`. Keep the private key, server fingerprint file, and real configuration
outside Git with ACL access limited to SYSTEM and Administrators.

The transfer is key-based, non-interactive, and requires strict host-key verification.
Plain FTP is intentionally unsupported because it exposes credentials and backup data in
transit. Configure remote retention (recommended: 90 days) at the storage provider; the
server retains its local copy for 30 days. With the current scheduled task, transfer runs
after every successful nightly backup at 02:15.

Run a backup manually:

```powershell
& .\ops\windows\Backup-PostgreSql.ps1
```

Prove the latest backup can be restored:

```powershell
& .\ops\windows\Test-PostgreSqlRestore.ps1
```

The restore test creates an isolated temporary database, validates migrations and seeded
locales, and removes the temporary database in a `finally` block.

## Production health

`Test-ProductionHealth.ps1` verifies the required services, public locale pages, admin
CSRF endpoint, free disk space, and certificate lifetime. It returns nonzero when any
check fails and emits JSON suitable for monitoring.

The scheduled wrapper writes the latest result to
`C:\ProgramData\Peletnapechkai\Health\latest.json` and appends compact history records
without storing credentials. Failures are also recorded in the Windows Application
event log with source `PeletnapechkaiHealth`.

```powershell
& .\ops\windows\Test-ProductionHealth.ps1
```

Secrets, database passwords, certificate private keys, and Owner bootstrap values must
never be added to these scripts or committed to Git.

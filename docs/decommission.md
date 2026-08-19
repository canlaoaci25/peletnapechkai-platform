# Decommission Plan

## Goal

Keep the repository public-ready while allowing safe removal of the current Windows-hosted runtime and infrastructure.

## Important order

1. **PHASE A (discovery)**
   - Identify all runtime entities.
   - Export inventory and evidence.
   - Do not delete during this phase.

2. **PHASE B (removal planning)**
   - Validate that each resource belongs to BOECL/Peletnapechkai.
   - Mark shared/shared-risk items as `REVIEW REQUIRED`.

3. **PHASE C (execution)**
   - Stop services and scheduled tasks.
   - Remove production components and files.
   - Verify cleanup using `scripts/verify-decommission.ps1`.

## Discovery checklist

- Windows services: `PeletnapechkaiWeb`, `BoeclStagingWeb`, `postgresql-x64-18`, `W3SVC`
- IIS sites: `Default Web Site`, `Peletnapechkai API`, `BOECL Staging`, `BOECL Staging API`
- App pools: `PeletnapechkaiApiPool`, `BoeclStagingApiPool`
- Scheduled tasks:
  - BOECL - Staging Health
  - BOECL Autonomous Improvement
  - BOECL Autonomous Watchdog
  - BOECL Codex Automation Worker
  - BOECL Continuity Supervisor
  - BOECL Hourly Sitemap Text
  - BOECL Weekly Quality Audit
  - Peletnapechkai - Production Health
  - Peletnapechkai - PostgreSQL Backup
- Automation scripts and state directories in:
  - `C:\ProgramData\Peletnapechkai`
  - `C:\ProgramData\BOECL`
  - `C:\ProgramData\WindowsPowerShell\` generated job artifacts
- Deployment roots:
  - `C:\inetpub\peletnapechkai\`
  - `C:\inetpub\boecl-staging\`

## Recommended non-destructive removal order

1. Stop scheduled tasks (to prevent re-spawn).
2. Stop services (`PeletnapechkaiWeb`, `BoeclStagingWeb`, `postgresql-x64-18`, `W3SVC` if dedicated).
3. Stop IIS application pools/sites.
4. Remove IIS configuration entries.
5. Remove service/worker scripts and scheduled-task artifacts.
6. Remove local DB and backup artifacts after validating retention.
7. Remove deployment directories and local media/data directories.
8. Verify no listening ports and no project tasks remain.

## Verification

Run:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\verify-decommission.ps1
```

## Shared resources

Do not remove unrelated services, user profiles, or system components unless confirmed project-specific.

## Documentation and backups

Before executing Phase C, export:

- service/task list
- last deployment snapshots
- backup catalog
- DNS/settings reference
- secrets revocation notes

## Final external closeout (manual)

Repository and host cleanup is complete for project-resident artifacts when
`scripts/verify-decommission.ps1` passes.

Before marking decommission as fully complete, execute:

- `docs/decommission-closeout-checklist.md`
- GitHub actions secrets/variables and webhook cleanup (operator controlled)
- Search Console / analytics / ad / DNS and TLS final handover actions

## Notes

This repository only manages source and scripts. Physical resource deletion should be done in the target Windows environment with explicit operator confirmation.

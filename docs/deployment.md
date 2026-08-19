# Deployment (Windows / IIS)

## Current model

- IIS handles public HTTPS termination and request routing.
- Next.js is hosted as a Windows service:
  - `PeletnapechkaiWeb` for production
  - `BoeclStagingWeb` for staging
- API is hosted as IIS application:
  - `Peletnapechkai API`
  - `BOECL Staging API`
- Database: PostgreSQL 18 service (`postgresql-x64-18`).
- Logs and paths are under `C:\ProgramData\Peletnapechkai` and `C:\inetpub` deployment trees.

## Deployment inputs

- Secrets and connection strings are injected in host configuration (not in repository).
- Service endpoints and directories are controlled by PowerShell scripts in `ops/windows`.

## Production promote sequence (documented)

1. Build and package web release.
2. Health and smoke checks.
3. Atomic directory swap and release journal entry.
4. API migration window and health verification.
5. Update script-controlled status files.

Scripts:

- `ops/windows/Deploy-NextWebRelease.ps1`
- `ops/windows/Deploy-AspNetApiRelease.ps1`
- `ops/windows/Promote-BoeclRelease.ps1`
- `ops/windows/Test-ProductionHealth.ps1`
- `ops/windows/Test-StagingHealth.ps1`
- `ops/windows/Test-PublicExperience.ps1`

## Required checks

- Health endpoint returns OK.
- API swagger/public status endpoint reachable.
- SEO surfaces (`sitemap.txt`, `robots`, locale routes) return expected statuses.
- `www` redirection and HTTPS redirection behavior works.

## CI/CD

Only `.github/workflows/ci.yml` exists today and is CI-oriented (lint/typecheck/build/test).
There is no deployment workflow in-repo for auto-production at this point.

## Stop/start reference

Use the decommission/deactivation workflow only when intentionally pausing the service.
Re-enable services and IIS components only after a clear restart playbook and secret validation.
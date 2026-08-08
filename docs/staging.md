# Staging environment

The isolated staging environment is available at
`https://staging.peletnapechkai.com`. It uses a separate PostgreSQL database, IIS API
site, application pool, Next.js Windows service, ports, deployment directories, and Data
Protection key ring. Production data and authentication cookies are not shared.

## Runtime map

- Public IIS site: `BOECL Staging`
- Gateway: `C:\inetpub\boecl-staging\gateway`
- Next.js service: `BoeclStagingWeb` on `127.0.0.1:3001`
- API IIS site/app pool: `BOECL Staging API` / `BoeclStagingApiPool`
- API endpoint: `127.0.0.1:5081`
- Database: `peletnapechkai_staging`
- Data Protection keys: `C:\ProgramData\Peletnapechkai\StagingDataProtectionKeys`

HTTP redirects to HTTPS. Let's Encrypt renewal is managed by win-acme. Every response
from the staging gateway includes `X-Robots-Tag: noindex, nofollow, noarchive`.

## Promotion gate

Pull requests and `main` pushes run web lint, type checking, production build, dependency
audits, EF migration consistency, API tests, and the .NET Release build in GitHub Actions.
Before production promotion, run the same checks locally and `Test-StagingHealth.ps1`.
Do not deploy if staging health, migration checks, or required reviews fail.

## Rollback

Keep the previous web and API release directories until the promoted release passes live
health checks. To roll back, stop the affected service/app pool, restore the previous
artifact directory, start it, and run both staging and production health scripts. Database
rollback is forward-only by default: restore the verified pre-deployment backup when a
schema change cannot be corrected safely with a new migration.

Secrets and real connection strings remain in IIS configuration or ACL-protected
ProgramData files and must not enter GitHub Actions logs or repository files.

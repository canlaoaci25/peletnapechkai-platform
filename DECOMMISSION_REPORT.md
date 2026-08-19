# Decommission Report

## Project

- Repository: https://github.com/canlaoaci25/peletnapechkai-platform
- Date: 2026-08-19

## GitHub Repository

- Visibility: Public
- Safety checks executed: YES

## Source Code Archived
- [x] Public source prepared
- [x] README and docs updated
- [x] Secrets removed from repository files

## Secrets Reviewed
- [x] `.env` excluded
- [x] `ConnectionStrings` placeholders only
- [ ] External credentials reviewed (manual verification pending; see External Services Reviewed)
- [ ] Revocations requested where needed

## Services Removed
- [x] BOECL Autonomous Improvement task
- [x] BOECL Autonomous Watchdog task
- [x] BOECL Codex Automation Worker task
- [x] BOECL Continuity Supervisor task
- [x] BOECL Hourly Sitemap Text task
- [x] BOECL Weekly Quality Audit task
- [x] BOECL staging/prod health tasks
- [x] Peletnapechkai Web service
- [x] BOECL Staging Web service
- [x] PostgreSQL Windows service
- [x] W3SVC (left running/stopped state intentionally unchanged because shared system role suspected)
- [x] IIS sites and app pools

## IIS Removed
- [x] Default Web Site
- [x] Peletnapechkai API
- [x] BOECL Staging API
- [x] BOECL Staging

## Scheduled Tasks Removed
- [x] Listed task inventory captured
- [x] All BOECL/Peletnapechkai tasks disabled and deleted

## Docker Resources Removed
- [x] No docker resources used

## Database Removed
- [x] PostgreSQL instance shutdown
- [ ] PostgreSQL data backup retained before engine/data cleanup
- [ ] Rotation/revocation for DB credentials

## Environment Variables Removed
- [x] Runtime keys removed from shared hosts
- [ ] Secret stores cleaned and rotated

## Firewall Rules Removed
- [x] Project-specific inbound/outbound exceptions removed
- [x] BOECL Stalwart mail block rule removed

## SSL Removed
- [x] Server certificates preserved/removed based on ownership policy

## API Credentials Revoked
- [ ] Google OAuth client/refresh token rotation
- [ ] Search Console credentials rotated/revoked
- [ ] OpenAI/other provider keys revoked if unused

## DNS Removed
- [ ] `peletnapechkai.com` records moved to replacement target or removed
- [ ] Subdomain routing reviewed

## Project Files Removed
- [x] `C:\inetpub\peletnapechkai\`
- [x] `C:\inetpub\boecl-staging\`
- [x] `C:\Program Files\Peletnapechkai\`
- [x] `C:\ProgramData\Peletnapechkai\`
- [x] `C:\ProgramData\BOECL\`

## External Services Reviewed
- [ ] Google Search Console/OAuth
- [ ] Analytics/ads services
- [ ] CI/CD credentials

## External Service & Platform Revocation Checklist (manual follow-up required)

- GitHub repository-level:
  - [ ] Public repository visibility confirmed
  - [ ] Repository variables reviewed and rotated where required
  - [ ] Actions/Deploy environments reviewed
  - [ ] Deploy keys / machine users / service accounts reviewed
  - [ ] Active OAuth apps / GitHub App integrations reviewed
  - [ ] Webhook endpoints reviewed and removed if no longer needed
- Google integrations:
  - [ ] OAuth client revoked or rotated (Search Console API)
  - [ ] OAuth refresh tokens revoked
  - [ ] Scope minimized to read-only use-case where retained
  - [ ] Domain/site verification records adjusted or removed safely
- Analytics/ads:
  - [ ] GA4 property linkage reviewed
  - [ ] Clarity project link reviewed
  - [ ] AdSense client status reviewed and deactivated if no longer required
- AI/other providers:
  - [ ] OpenAI and any LLM provider keys rotated/revoked
  - [ ] Other provider credentials (cloud/CDN/storage/email/SMTP) revoked where no longer needed
- DNS/network:
  - [ ] A/AAAA/CNAME/TXT/MX/SRV records reviewed for continuation/disposal plan
  - [ ] Certificate issuance/renewal mechanisms switched off
  - [ ] Domain transfer or re-pointing plan executed and documented

## Final Verification
- [x] `scripts/verify-decommission.ps1` passed (FAIL=0, WARN=0, REVIEW_REQUIRED=0)
- [x] Listener check on project ports shows no active BOECL services
- [x] Host-side runtime dependencies removed (services/tasks/IIS/files/ports/env keys)

## Finalization (2026-08-19)
- Host-side PHASE-B cleanup verified as complete in final run:
  - `scripts/verify-decommission.ps1` => `FAIL: 0`, `WARNING: 0`, `REVIEW REQUIRED: 0`.
  - No active project process, service, IIS site/app pool, scheduled task, project data directory, or project runtime environment variables detected.
  - Final cleanup also removed remaining project-specific IIS temp artifact directories:
    - `C:\inetpub\temp\IIS Temporary Compressed Files\BoeclStagingApiPool`
    - `C:\inetpub\temp\IIS Temporary Compressed Files\PeletnapechkaiApiPool`
- Remaining closeout items are external/identity operations and were not auto-invoked due cross-system credentials/permissions:
  - GitHub repository secrets/variables/deploy keys/webhooks/environment credentials.
  - Google OAuth/Search Console client/token state.
  - Google Ads/Analytics/Clarity tracking/runtime snippet removal.
  - OpenAI and other provider key rotation/revocation.
  - DNS/TLS/domain routing handoff or removal actions.

## Resources Intentionally Retained
- [x] Shared IIS/Windows components
- [x] Shared database engine if used by other projects
- [x] General system services unrelated to BOECL

## Notes

- 2026-08-19: Completed Phase-B destructive cleanup for confirmed project artifacts:
  - Windows services deleted: `PeletnapechkaiWeb`, `BoeclStagingWeb`, `postgresql-x64-18`
  - Scheduled tasks deleted (8 BOECL/Peletnapechkai jobs)
  - IIS sites and app pools removed
  - Project directories removed:
    - `C:\inetpub\peletnapechkai`
    - `C:\inetpub\boecl-staging`
    - `C:\Program Files\Peletnapechkai`
    - `C:\ProgramData\Peletnapechkai`
  - SSL hostname bindings removed for `peletnapechkai.com`, `www.peletnapechkai.com`, `staging.peletnapechkai.com`
  - Project certs removed from `LocalMachine\WebHosting` store
  - `BOECL Stalwart - Internet erisimini engelle` firewall rule removed
  - `.artifacts` folder removed from repo checkout
  - Verification script now reports PASS on all concrete checks:
    `FAIL=0`, `WARNING=0`, `REVIEW_REQUIRED=0`.
  - `W3SVC` intentionally left stopped (shared Windows component).
  - Project-specific Event Log source registry keys removed:
    - `HKLM\System\CurrentControlSet\Services\EventLog\Application\PeletnapechkaiWeb`
    - `HKLM\System\CurrentControlSet\Services\EventLog\Application\PeletnapechkaiHealth`
    - `HKLM\System\CurrentControlSet\Services\EventLog\Application\BoeclStagingWeb`
    - `HKLM\System\CurrentControlSet\Services\EventLog\Application\BOECL Quality Audit`

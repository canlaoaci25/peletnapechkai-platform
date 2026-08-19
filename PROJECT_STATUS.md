# Project Status

## What works today

- Multi-locale publishing (`tr-TR`, `en-US`, `de-DE`, `fr-FR`).
- Role-based auth and editorial workflow (draft/review/schedule/publish/archive).
- Locale-aware routing and SEO metadata (canonical/hreflang/sitemap/feed behavior).
- Local media pipeline with local file storage and cleanup protections.
- Windows/IIS production and staging deployment scripts.
- Background automation framework for content and SEO operations.
- Readiness and health scripts for staging/production.

## Partial / incomplete

- Decommission is prepared via docs and script, but full infrastructure removal on production host must still be executed manually with confirmation.
- External service state (Google OAuth, Search Console, ad/integrations) depends on environment secrets and may be disabled unless configured.
- Some legacy audit files and autonomous modules are extensive and may need consolidation for new contributors.

## Not working / pending

- No active public runtime service on current host after shutdown.
- No ongoing external integrations can be validated until credentials/secrets are reconfigured.
- GitHub Actions deployment is documented but not currently active in this repository context.

## Known bugs / technical debt

- Legacy autonomous modules and scripts should be reviewed before re-enabling full automated execution.
- Documentation has both root docs and historical in-depth cycle notes; contributors should prioritize canonical docs listed here.
- No CDN/object storage currently in use; local media limits scale and resilience.

## Security checklist before restart

- Validate secret storage and revocation.
- Confirm service identities and ACLs.
- Re-run backup/restore and deployment health checks.
- Verify environment variables are present in deployment host.
- Audit task schedules and automation controls before resuming.

## Why project was paused

The codebase is stable but intentionally archived to allow safe public continuity.
Current work remains significant and the platform should be treated as a continuation-ready baseline, not a fully production-locked final state.
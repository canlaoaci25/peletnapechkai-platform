# BOECL Roadmap

This roadmap captures what is currently intended for future work and explicitly separates implemented foundations from future items.

## Completed foundations

- Multi-locale public and admin platform with editorial workflow.
- ASP.NET Core API + Next.js web stack.
- EF Core/PostgreSQL persistence and migration process.
- API authentication/authorization and audit logging.
- SEO metadata primitives (canonical/hreflang, sitemap/feed behavior, JSON-LD where applicable).
- Deployment and recovery scripts for Windows/IIS.
- Automation/worker architecture and monitoring hooks.

## Ongoing phase: Decommission readiness (archive phase)

1. Verify shutdown of all runtime and automation surfaces.
2. Produce a complete reusable cleanup checklist.
3. Prepare clean reinstall/deploy path via docs and script.
4. Keep public repo safe for public contributors.

## Future cycles (planned)

### Cycle A: Infrastructure resilience

- Add cache layer (Redis or equivalent) after profiling shows need.
- Add queue abstraction for long-running jobs.
- Add hardened deploy secrets process for environment-specific secret stores.
- Add full CI/CD promotion with deployment visibility.

### Cycle B: Content and SEO quality

- Strengthen source verification quality gates.
- Improve multilingual content parity checks.
- Improve schema consistency coverage for all public content surfaces.
- Expand image and media quality policy with accessibility audit coverage.

### Cycle C: Product operations

- Notification center (in-app web push + optional email channel).
- More robust admin telemetry and progress views.
- Advanced traffic growth and search-gap dashboard with historical snapshots.

### Cycle D: Runtime hardening

- Add centralized rate-limits and abuse controls for public APIs.
- Add CSP/secure headers in report mode first, then enforce.
- Add CDN/CDN-like caching policy for static assets.

### Cycle E: Tooling and governance

- Document owner decision workflow and governance in a release-safe changelog.
- Add contributor playbooks for automation-safe upgrades.
- Add explicit migration and rollback runbooks for every major release.

## Out of scope (deliberate)

- Open public monetization experiments requiring additional legal/financial systems.
- Major architecture migration (e.g., cloud-native microservices) without prior staged proof.
- Full feature unlock until security and secrets model is validated by a maintainer.
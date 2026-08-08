# BOECL integrations

Active: verified Google Search Console property, sitemap/robots discovery, Let's Encrypt
renewal, PostgreSQL backup/restore checks, production/staging health schedules, GitHub and
GitHub Actions quality gates.

Ready but disabled until an account identifier is supplied: GA4
(`NEXT_PUBLIC_GA_MEASUREMENT_ID`), Clarity (`NEXT_PUBLIC_CLARITY_PROJECT_ID`), Bing and
Google verification metadata, IndexNow (`INDEXNOW_KEY`), and advertising. GA4/Clarity are
not emitted before optional consent; Clarity advertising consent remains denied. Invalid
identifiers are rejected. IndexNow submits only same-origin sitemap URLs.

Candidates needing a separate account or measured need: Sentry/OpenTelemetry, Cloudflare
CDN/WAF/R2, an off-server uptime probe, transactional email, Redis, dedicated search, and
queues.

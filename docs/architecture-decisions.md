# Architecture decisions

## ADR-001: Split frontend and API

Status: Accepted — 2026-08-04

The public website and administration UI use Next.js App Router. Business operations and
data access use an ASP.NET Core 10 Web API. This keeps server-rendered publishing concerns
separate from the domain and provides a future API surface for other clients.

## ADR-002: Locale and region in the URL

Status: Accepted — 2026-08-04

Every public route starts with a locale-region segment. The current interface values are
`tr-TR`, `en-US`, `de-DE`, and `fr-FR`, with `tr-TR` as the default. IP detection may suggest a region but
must not make content inaccessible or force a redirect.

## ADR-003: Editorially controlled localization

Status: Accepted — 2026-08-04

Localized articles belong to a shared article group but remain independent publishable
records. AI can prepare drafts and suggestions; an authorized editor must approve them.

## ADR-004: Start without distributed infrastructure

Status: Accepted — 2026-08-04

The MVP uses PostgreSQL and process-level caching. Redis, a separate search engine,
containers, and orchestration are introduced only after measurements show a need.

## ADR-005: Preserve the current production site

Status: Accepted — 2026-08-04

Development and acceptance happen in a separate staging environment. Existing IIS sites,
bindings, files, and DNS are not changed until a tested migration and rollback plan exists.

## ADR-006: Separate database ownership and runtime access

Status: Accepted — 2026-08-04

EF Core migrations use a dedicated database owner account. The running API uses a
separate account limited to connecting, reading, and changing application data. The
database listens only on localhost during development, and credentials stay outside the
repository in .NET User Secrets.

## ADR-007: Server-side cookie identity for administration

Status: Accepted — 2026-08-04

The administration application uses ASP.NET Core Identity with an HTTP-only, SameSite
cookie rather than storing bearer tokens in browser storage. State-changing endpoints
require an antiforgery token. Login is protected by both IP-partitioned rate limiting and
account lockout. API authentication failures return `401` or `403` and never redirect to
an HTML login page.

No user or password is seeded by a migration. The first Owner is created once through an
explicit bootstrap command whose values come from secret configuration.

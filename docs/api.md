# BOECL API Documentation

This document summarizes the current API surface that exists in the repository.

## Runtime stack

- ASP.NET Core 10 minimal API hosted by `apps/api`.
- Endpoint registration is in `Program.cs`.
- Runtime uses hosted workers, Entity Framework Core, and PostgreSQL.

## Base URL

- Production/API base: configured by `NEXT_PUBLIC_API_URL` and `ASPNETCORE_URLS`.
- The app also exposes a health endpoint at `/health`.
- OpenAPI is enabled only in Development.

## Public API proxy

`apps/web` forwards some calls through Next.js API routes:

- `POST /api/admin/...` (Next.js) -> backend admin/public proxy.
- `POST/GET /api/admin/[...path]` accepts only allowlisted paths:
  - `auth/*`, `articles/*`, `users/*`, `supporting/*`, `media/*`, `status`, `knowledge/*`, `locales/*`, `automation/*`, `homepage/*`, `development/*`, `account/*`.
- `GET /api/engagement` -> `/api/v1/public/{locale}/articles/{slug}/engagement`.
- `GET /api/media/{assetId}` -> `/api/v1/public/media/{assetId}`.
- `POST /api/web-vitals` -> `/api/v1/public/web-vitals`.

## Endpoint map (high level)

- `/health`
  - `GET /health` API liveness.

- Authentication
  - `POST /api/v1/auth/login`
  - `POST /api/v1/auth/2fa/...`
  - `POST /api/v1/auth/logout`
  - `GET  /api/v1/auth/session`
  - `GET  /api/v1/auth/csrf`
  - invitation flows under `/api/v1/auth/complete-invitation`

- Users and roles
  - `GET /api/v1/admin/users`
  - `POST /api/v1/admin/users/invite`
  - role/active-state/session-revoke endpoints for user administration.

- Editorial content
  - `/api/v1/admin/articles` (list/get/create/update/submit/schedule/publish/archive and related resources).
  - `/api/v1/admin/articles/{articleId}/collaboration/*` (comments, tasks, checklist, etc.).
  - `/api/v1/admin/editorial/...` command center endpoints.

- Supporting content
  - `/api/v1/admin/supporting/categories`, `tags`, `sources`, and author endpoints.
  - `/api/v1/admin/media` upload/list/admin operations.

- Taxonomy and locale operations
  - `/api/v1/admin/locales` (locale catalog and per-locale work assignment).
  - `/api/v1/admin/locales/work/*` for locale assignment workflows.

- Automation
  - `/api/v1/admin/automation/*` (content jobs, visual quality actions, ready-content creation, automatic content settings).
  - `/api/v1/internal/automation-worker/*` for worker handshakes, progress, results, heartbeats, and retries.
  - Additional automation/quality worker endpoints are grouped under `/api/v1/admin/automation` and `/api/v1/internal/automation-worker`.

- Knowledge + homepage/public modules
  - `/api/v1/admin/knowledge` (knowledge vault entries and links).
  - `/api/v1/admin/homepage` get/save homepage curation state.

- Account & membership APIs
  - `/api/v1/account` profile/saved/following/reading-progress/push.

- Admin/system status and traffic
  - `/api/v1/admin/status`
  - `/api/v1/admin/development/*` (including autonomous mode controls)
  - `/api/v1/admin/web-vitals`
  - `/api/v1/admin/traffic/{locale}`

- Public content
  - `/api/v1/public/{locale}/articles`
  - `/api/v1/public/{locale}/articles/search`
  - `/api/v1/public/{locale}/articles/{slug}`
  - `/api/v1/public/{locale}/archives*`
  - `/api/v1/public/{locale}/sources*`
  - `/api/v1/public/media/{assetId}`
  - `/api/v1/public/{locale}/articles/{slug}/related`
  - `/api/v1/public/{locale}/homepage`
  - `/api/v1/homepage` internal frontend-facing endpoint aliases where applicable.

- System utility
  - `GET /api/v1/locales` (enabled locales with country mapping, used by web middleware).
  - `POST /api/v1/public/web-vitals`

## Public contract rules

- Responses use JSON except media and feed/sitemap/text routes.
- Draft or unpublished states are excluded from public discovery endpoints.
- Locale-aware routes are strict and must use supported locale codes:
  - `tr-TR` (default), `en-US`, `de-DE`, `fr-FR`.
- `/api/v1/public/{locale}/articles/{slug}` includes edition links only for existing published localizations.

## Authentication and authorization

- Cookie auth is used with ASP.NET Core Identity.
- Policies:
  - `ManageUsers` (`Owner`, `Admin`)
  - `ManageEditorial` (`Owner`, `Admin`, `Editor`)
  - `WriteContent` (`Owner`, `Admin`, `Editor`, `Author`)
  - `TranslateContent` (`Owner`, `Admin`, `Editor`, `Translator`)
  - `ManageSeo` (`Owner`, `Admin`, `Editor`, `Seo`)
- Admin/public account endpoints are grouped under `/api/v1/admin` and mostly require auth + anti-forgery token on mutations.
- CSRF header: `X-CSRF-TOKEN`.

## Rate limits

Configured in `IdentityServiceExtensions`:

- `login` policy: sliding 10 requests / 5 minutes per remote address.
- `public-engagement` policy: sliding 30 requests / minute per remote address.

When limits are exceeded, the API returns `429`.

## Background processing model

There is no external queue service in use.

- `ScheduledPublishingWorker` (publishing cadence/state transitions).
- `AutomaticContentWorker` (automation jobs).
- `WebVitalRetentionWorker` (rolling metric cleanup).
- Internal automation APIs under `/api/v1/internal/automation-worker/*` coordinate workers and checkpointed work.

Jobs and worker reports are stored in PostgreSQL tables (no dedicated Redis/message broker today).

## Error/observability behavior

- Standard HTTP status codes are used.
- Administrative worker endpoints provide control/status updates for long-running operations.
- Long-running automation results are recorded in API persistence and surfaced to admin UI and logs.

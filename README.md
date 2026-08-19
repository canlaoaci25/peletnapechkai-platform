# BOECL Publishing Platform

BOECL is a multilingual publishing platform built with a Next.js App Router frontend and ASP.NET Core 10 API, focused on editorial-first content production across Turkish, English, German, and French locales.

## Project status

Development is currently paused by the owner. The repository is prepared for handoff so external contributors can continue work safely.

## Current state

- Core architecture and production deployment are present.
- Editorial/content workflows, locale-aware publishing, and automated background jobs exist.
- SEO structure includes canonical/hreflang, robots, sitemap/feed surfaces.
- A full cleanup/decommission plan is now prepared, but destructive removal on Windows is not automatically executed by this repo alone.

## Project goal

Deliver a maintainable, locale-aware content platform with:

- Editorial governance by role
- Multi-language publishing and URL localization
- Search-aware and automation-aware discovery surfaces
- Safe extension points for additional SEO/traffic providers

## What this repository contains

- `apps/web`: Next.js frontend and admin interface.
- `apps/api`: ASP.NET Core API and authorization/business logic.
- `tests/api`: API test suite.
- `docs`: Architecture, operations and quality documentation.
- `ops/windows`: PowerShell operational scripts for automation and deployment.

## Project status overview

- Working stack: ✅ implemented and documented
- Deployment: ✅ documented for Windows/IIS
- Public run readiness: ⚠️ runtime currently stopped intentionally (decommission phase)

## Architecture summary

- Frontend: Next.js `16.3.0` + React `19.2.8`.
- Backend: ASP.NET Core `10.0` + minimal API endpoint model.
- Database: PostgreSQL + Entity Framework Core `10` + Npgsql provider.
- Auth: ASP.NET Core Identity with cookie authentication and CSRF.
- Background/background-like work: `BackgroundService` workers + scheduled Windows tasks.

## Requirements

- Node.js 24 (or matching supported runtime)
- npm 11+
- .NET SDK 10
- PostgreSQL 18 for local/runtime environments

## Environment

Copy `.env.example` to your local env file only for local development and never commit secrets.

Required public variables:

- `NEXT_PUBLIC_SITE_URL`
- `NEXT_PUBLIC_API_URL`

Required runtime variables:

- `ASPNETCORE_ENVIRONMENT`
- `ASPNETCORE_URLS`
- `ConnectionStrings__Database`
- `ConnectionStrings__DatabaseMigration`
- `DataProtection__KeysPath`

## Common commands

```powershell
npm run lint
npm run typecheck
npm run build:web
dotnet test Peletnapechkai.slnx
dotnet build Peletnapechkai.slnx --configuration Release
```

Database commands are documented in [`docs/database.md`](docs/database.md).

## Development and deployment references

- [SECURITY.md](SECURITY.md)
- [PROJECT_STATUS.md](PROJECT_STATUS.md)
- [ROADMAP.md](ROADMAP.md)
- [CHANGELOG.md](CHANGELOG.md)
- [CONTRIBUTING.md](CONTRIBUTING.md)
- [docs/installation.md](docs/installation.md)
- [docs/deployment.md](docs/deployment.md)
- [docs/architecture.md](docs/architecture.md)
- [docs/decommission.md](docs/decommission.md)
- [docs/api.md](docs/api.md)
- [docs/seo.md](docs/seo.md)

## License

MIT (see [`LICENSE`](LICENSE)).

> Active development by the original author is currently paused. The repository remains public for learning, experimentation, forks and community contributions.

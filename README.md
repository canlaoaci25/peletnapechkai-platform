# Peletnapechkai Platform

Peletnapechkai is a multilingual, multi-region publishing platform. The initial release
supports Turkish/Türkiye, English/United States, and German/Germany.

## Projects

- `apps/web`: Next.js frontend and future administration interface.
- `apps/api`: ASP.NET Core Web API.
- `tests/api`: API unit tests.
- `docs`: Architecture decisions and project documentation.

## Requirements

- Node.js 24 LTS
- npm 11 or later
- .NET SDK 10
- PostgreSQL 18

## Local development

```powershell
npm run dev:web
dotnet run --project apps/api/Peletnapechkai.Api.csproj
```

The web application defaults to `http://localhost:3000`. The API launch profile uses the
URL shown by `dotnet run`; `.env.example` documents the intended shared defaults.

## Quality checks

```powershell
npm run lint
npm run typecheck
npm run build:web
dotnet test Peletnapechkai.slnx
dotnet build Peletnapechkai.slnx --configuration Release
```

Copy `.env.example` to an untracked local environment file when configuration is needed.
Never commit credentials or production connection strings.

Database credentials are stored with .NET User Secrets during local development. Run
`dotnet tool restore` before using the migration commands documented in
[`docs/database.md`](docs/database.md).

Authentication uses server-side ASP.NET Core Identity cookies. See
[`docs/identity.md`](docs/identity.md) before creating the first Owner account or calling
authentication endpoints.

The Phase 4 editorial state machine is documented in
[`docs/editorial-workflow.md`](docs/editorial-workflow.md).

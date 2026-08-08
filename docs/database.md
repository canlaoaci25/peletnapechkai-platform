# Local database

Phase 2 uses PostgreSQL 18 with EF Core 10 and Npgsql. On the development server,
PostgreSQL listens only on `127.0.0.1` and `::1`; no database firewall port is open.

## Accounts and database

- `peletnapechkai_dev` is the local development database.
- `peletnapechkai_owner` owns the schema and is used only for migrations.
- `peletnapechkai_app` is the low-privilege runtime account. It cannot create tables.

Passwords and full connection strings must not be committed or copied into this file.
For local development they are stored in .NET User Secrets under the API project as
`ConnectionStrings:Database` and `ConnectionStrings:DatabaseMigration`.

## Migrations

Run these commands from the repository root:

```powershell
dotnet tool restore
dotnet tool run dotnet-ef migrations add MigrationName --project apps/api/Peletnapechkai.Api.csproj --startup-project apps/api/Peletnapechkai.Api.csproj
dotnet tool run dotnet-ef database update --project apps/api/Peletnapechkai.Api.csproj --startup-project apps/api/Peletnapechkai.Api.csproj
```

CI checks that the EF model has no changes missing from migrations.

## Database integration tests

Normal tests do not require PostgreSQL. To include the local database checks:

```powershell
$env:RUN_DATABASE_TESTS = 'true'
dotnet test tests/api/Peletnapechkai.Api.Tests.csproj --configuration Release
Remove-Item Env:RUN_DATABASE_TESTS
```

These tests verify connectivity, seeded locales and regions, and the runtime account's
schema restriction. They also verify read access to the Phase 2 publishing tables.

## Phase 2 publishing schema

Categories and tags are locale-specific. Authors, sources, and media assets are shared by
the article group. New image uploads are fully decoded, constrained to 40 megapixels,
and receive a maximum-1200-pixel WebP cover variant. The original remains
outside the web root. Media unused for at least 24 hours can be explicitly removed by an
authorized editor; referenced files are never cleanup candidates.
an article group. Revisions and SEO metadata belong to a localized article. Audit logs are
append-only at the application layer. JSON details and structured data use PostgreSQL
`jsonb`; callers must still treat stored content and metadata as untrusted input.

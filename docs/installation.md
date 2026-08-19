# Installation

## Requirements

- Node.js 24 LTS
- npm 11+
- .NET SDK 10.0
- PostgreSQL 18

## Clone and prepare

```bash
git clone https://github.com/canlaoaci25/peletnapechkai-platform.git
cd peletnapechkai-platform
```

## Environment

1. Copy `.env.example` to `.env.local` for local development.
2. Do not commit `.env*` files containing real values.
3. Configure connection strings in local secret stores for API as needed for local DB setup.

## Install

```powershell
npm install
npm run dev:web
```

## API setup

```powershell
dotnet restore Peletnapechkai.slnx
dotnet tool restore
```

Migrations:

```powershell
dotnet tool run dotnet-ef migrations add <Name> --project apps/api/Peletnapechkai.Api.csproj --startup-project apps/api/Peletnapechkai.Api.csproj
dotnet tool run dotnet-ef database update --project apps/api/Peletnapechkai.Api.csproj --startup-project apps/api/Peletnapechkai.Api.csproj
```

## Run locally

```powershell
# terminal 1
npm run dev --prefix apps/web

# terminal 2
npm run test --prefix apps/web

dotnet run --project apps/api/Peletnapechkai.Api.csproj
```

`api` default URL is configured by environment in `.env` and API startup config.

## Useful checks

```powershell
npm run lint
npm run typecheck
npm run build:web

dotnet test Peletnapechkai.slnx

dotnet build Peletnapechkai.slnx --configuration Release
```

## Troubleshooting

- If web cannot reach API, verify `NEXT_PUBLIC_API_URL` and `ASPNETCORE_URLS`.
- If DB migrations fail, verify DB role/connection string and `postgres` service availability.
- If admin endpoints fail, verify anti-forgery cookie/session requirements.
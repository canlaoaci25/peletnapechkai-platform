# Contributing to BOECL

Thanks for your interest in contributing. This project is currently archived/paused for continuation by others.

## Branching and setup

- Fork the repository
- Create a branch using a descriptive name
- Do not commit secrets or credentials
- Keep changes small and scoped to your task

## Development setup

```powershell
npm install
npm run lint
npm run typecheck
npm run build:web

dotnet restore Peletnapechkai.slnx
dotnet build Peletnapechkai.slnx --configuration Release
```

## Testing

```powershell
dotnet test Peletnapechkai.slnx
dotnet build Peletnapechkai.slnx --configuration Release
```

Local database tests can be enabled with `RUN_DATABASE_TESTS=true` for API checks.

## Contribution guidelines

- Follow existing architecture and naming patterns.
- Add/extend tests for behavior changes.
- Update docs for user-visible changes.
- Keep `apps/web` and `apps/api` concerns separated.
- Do not touch `.env`, secrets, logs, or user data.

## Commit and PR

- Use clear commit titles and reference related audit/issue ticket IDs.
- Include:
  - what changed
  - manual verification performed
  - how to reproduce
  - any known risks

## Security

- Never add credentials to `.env` committed files.
- Do not post passwords, API keys, tokens, or internal URLs in public issues.
- Use `.env.example` placeholders only.
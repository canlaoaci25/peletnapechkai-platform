# Identity and authorization

The API uses ASP.NET Core Identity with PostgreSQL and a server-side authentication
cookie. The initial roles are `Owner`, `Admin`, `Editor`, `Author`, `Translator`, and
`SEO`. Migrations seed only these role names and stable IDs; they never contain a user,
password, token, or email address.

## Security behavior

- Authentication cookies are HTTP-only, SameSite Lax, non-persistent, and valid for at
  most eight hours with sliding renewal.
- Non-development environments require HTTPS cookies.
- Email confirmation is required before login.
- Passwords require at least 14 characters and four distinct characters, including
  uppercase, lowercase, numeric, and non-alphanumeric characters.
- Five failed attempts lock an account for 15 minutes.
- Login is additionally limited to ten attempts per five minutes per remote IP.
- State-changing authentication calls require the `X-CSRF-TOKEN` header.
- Successful login, logout, and Owner bootstrap actions create append-only audit records.

Forwarded proxy headers must be configured before trusting client IP rate-limit
partitions in staging or production. Persistent Data Protection keys must also be bound
to the final Windows service identity before deployment.

## Authentication flow

1. Call `GET /api/v1/auth/csrf` with cookies enabled and read the returned `token`.
2. Send that token as `X-CSRF-TOKEN` when calling `POST /api/v1/auth/login`.
3. Continue sending cookies for authenticated API requests.
4. Use `GET /api/v1/auth/session` to read the current account and roles.
5. Send a current CSRF token to `POST /api/v1/auth/logout`.

## Create the first Owner

Only run this after the Identity migration is applied. Store these values in the API
project's .NET User Secrets; never commit or write real values into project documents:

```powershell
dotnet user-secrets set "OwnerBootstrap:Email" "OWNER_EMAIL" --project apps/api/Peletnapechkai.Api.csproj
dotnet user-secrets set "OwnerBootstrap:DisplayName" "OWNER_DISPLAY_NAME" --project apps/api/Peletnapechkai.Api.csproj
dotnet user-secrets set "OwnerBootstrap:Password" "ONE_TIME_STRONG_PASSWORD" --project apps/api/Peletnapechkai.Api.csproj
dotnet run --project apps/api/Peletnapechkai.Api.csproj -- --bootstrap-owner
dotnet user-secrets remove "OwnerBootstrap:Email" --project apps/api/Peletnapechkai.Api.csproj
dotnet user-secrets remove "OwnerBootstrap:DisplayName" --project apps/api/Peletnapechkai.Api.csproj
dotnet user-secrets remove "OwnerBootstrap:Password" --project apps/api/Peletnapechkai.Api.csproj
```

The command refuses to run if any user already exists. It creates one confirmed Owner,
assigns the Owner role, writes an audit record, and exits without starting the web API.
The placeholders above must be replaced interactively and should not be retained in shell
history on a shared machine.

## Authorization policies

- `ManageUsers`: Owner, Admin
- `ManageEditorial`: Owner, Admin, Editor
- `WriteContent`: Owner, Admin, Editor, Author
- `TranslateContent`: Owner, Admin, Editor, Translator
- `ManageSeo`: Owner, Admin, Editor, SEO

User invitation, role administration, two-factor enrollment, recovery codes, and forced
session revocation remain in the next Phase 3 delivery slice.

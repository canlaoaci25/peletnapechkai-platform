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
- Authenticator-app 2FA and one-time recovery codes are supported.
- Security-stamp rotation invalidates existing sessions within the five-minute validation
  interval.

Only loopback IIS proxies are trusted for `X-Forwarded-For` and `X-Forwarded-Proto`, with
one forwarded hop. Do not enable the unrestricted forwarded-headers environment switch.

Outside Development, `DataProtection:KeysPath` is mandatory. On Windows, persisted keys
are encrypted at rest using machine-level DPAPI. This server uses
`C:\ProgramData\Peletnapechkai\DataProtectionKeys`; inheritance is disabled and only
SYSTEM and Administrators currently have access. Before deployment, grant the final API
Windows service identity read/write access without broadening access to ordinary users.

## Authentication flow

1. Call `GET /api/v1/auth/csrf` with cookies enabled and read the returned `token`.
2. Send that token as `X-CSRF-TOKEN` when calling `POST /api/v1/auth/login`.
3. After login, request a new CSRF token because antiforgery tokens are bound to the
   current identity. Continue sending cookies for authenticated API requests.
4. Use `GET /api/v1/auth/session` to read the current account and roles.
5. Send a current CSRF token to `POST /api/v1/auth/logout`.

## Two-factor authentication

All state-changing calls below require a current CSRF token. Responses containing an
authenticator secret or recovery codes use `Cache-Control: no-store`.

1. Call `POST /api/v1/auth/2fa/setup` with the current password. Add the returned URI to
   an authenticator application.
2. Call `POST /api/v1/auth/2fa/enable` with a current six-digit authenticator code. Store
   the returned recovery codes once in a secure location.
3. After a password login returns `twoFactorRequired`, call
   `POST /api/v1/auth/login/2fa` with either an authenticator code or one recovery code.
4. Recovery codes can be replaced through `POST /api/v1/auth/2fa/recovery-codes`; the
   current password is required and old codes are invalidated.
5. `POST /api/v1/auth/2fa/disable` requires the current password, resets the authenticator
   key, rotates the security stamp, and signs out the session.

`POST /api/v1/auth/session/revoke-all` requires the current password, rotates the
security stamp, and signs out the current session. Other cookies become invalid at their
next security-stamp check.

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

## User management and invitations

Endpoints under `/api/v1/admin/users` require `ManageUsers` and CSRF protection for
changes. They support listing users, creating a one-time invitation, replacing roles,
activating/deactivating accounts, and forcing session revocation.

The invitation endpoint returns a one-time setup token with `Cache-Control: no-store`.
Until an email provider is introduced, an Owner/Admin must transfer the user ID and token
through a separate secure channel. The token must never be logged or placed in a URL.
`POST /api/v1/auth/complete-invitation` accepts the ID, token, and new password; successful
completion confirms the invited account.

Only an Owner can assign the Owner role. The last active Owner cannot be deactivated or
have the Owner role removed, and a user cannot deactivate their own account. Role changes
and account deactivation rotate the target user's security stamp.

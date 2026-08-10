# Windows Server and IIS deployment

The production deployment for `peletnapechkai.com` runs on Windows Server 2022.
IIS terminates HTTPS and reverse-proxies public traffic to the Next.js standalone
server. The ASP.NET Core API is hosted by IIS on loopback only.

## Runtime layout

- Public IIS site: `Default Web Site`
- Public gateway: `C:\inetpub\peletnapechkai\gateway`
- Next.js deployment: `C:\inetpub\peletnapechkai\web`
- Next.js Windows service: `PeletnapechkaiWeb`
- Next.js loopback endpoint: `http://127.0.0.1:3000`
- API IIS site: `Peletnapechkai API`
- API deployment: `C:\inetpub\peletnapechkai\api`
- API application pool: `PeletnapechkaiApiPool`
- API loopback endpoint: `http://127.0.0.1:5080`
- Data Protection keys: `C:\ProgramData\Peletnapechkai\DataProtectionKeys`
- Runtime logs: `C:\ProgramData\Peletnapechkai\Logs`

The database and application ports are not exposed through Windows Firewall.
Only IIS ports 80 and 443 are public.

## IIS extensions and TLS

- Microsoft URL Rewrite 2.1
- Microsoft Application Request Routing 3.0
- ASP.NET Core Module V2 from the .NET 10 Hosting Bundle
- Let's Encrypt certificate managed by win-acme

HTTP redirects permanently to HTTPS. The `www` hostname redirects permanently to
the apex hostname. Certificate renewal is handled by the scheduled win-acme task.

## Configuration and secrets

Production connection strings are applied to the API worker environment and are not
stored in this repository. Data Protection keys are protected with machine-level DPAPI.
The key directory grants access only to SYSTEM, Administrators, and the final API
application-pool identity.

The Next.js service uses these non-secret runtime values:

- `NODE_ENV=production`
- `HOSTNAME=127.0.0.1`
- `PORT=3000`
- `NEXT_PUBLIC_SITE_URL=https://peletnapechkai.com`
- `API_INTERNAL_URL=http://127.0.0.1:5080`

The API loopback site supplies the trusted forwarded HTTPS scheme before invoking
ASP.NET Core. The application itself accepts forwarded headers only from loopback.

## Deployment validation

After deployment or a service restart, verify:

1. `PeletnapechkaiWeb`, `W3SVC`, and `postgresql-x64-18` are running.
2. `http://peletnapechkai.com/` redirects to HTTPS.
3. `https://www.peletnapechkai.com/` redirects to the apex hostname.
4. `/tr-TR`, `/en-US`, `/de-DE`, and `/fr-FR` return successful pages.
5. `/tr-TR/admin/login` returns the login page.
6. `/api/admin/auth/csrf` returns HTTP 200 and a secure antiforgery cookie.

Never copy production credentials, owner bootstrap values, certificates, or private
keys into this document or the Git repository.

Changes must pass the isolated staging environment described in
[`staging.md`](staging.md) before production promotion.

# Security Policy

If you discover a security issue, do not publish it publicly.

## Reporting

- Use a private channel (GitHub Security Advisory if enabled) for vulnerabilities.
- Include reproduction steps and impact level.
- Do not include secrets in issue text.

## Handling and disclosure

- Confirm severity and impact first.
- Validate reproduction in a non-production environment.
- Coordinate fix and public disclosure window.

## Secret hygiene

- Do not store secrets in repository files.
- Use local environment/configuration stores or secret managers.
- Rotate exposed credentials immediately (database, API, OAuth, email, CI).

## Operational controls

- Production credentials should never be in `.env` checked into git.
- Prefer machine-level access controls for data-protection keys.
- Keep service accounts minimal and rotate where applicable.

## Dependency security

- Run dependency and vulnerability checks periodically.
- Remove unused third-party integrations that cannot be safely managed.
- Keep container/runner/tooling dependencies aligned with least privilege.

## Scope for this repo

- Search Console OAuth tokens
- Google API credentials
- OpenAI-related keys
- Database credentials
- SMTP credentials
- Service tokens used by automation jobs
- Deployment and signing secrets
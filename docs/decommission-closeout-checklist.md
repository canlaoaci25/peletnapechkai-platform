# Decommission Closeout Checklist (Manual Operators)

This checklist covers the remaining non-repository cleanup items after
`scripts/verify-decommission.ps1` passes.

## 1) GitHub side

- Confirm repository visibility status (public/archive as intended).
- In `Settings > Secrets and variables > Actions`, review all:
  - `Actions` secrets/variables
  - `Environment` secrets/variables
- Remove, rotate, or replace any token tied to old runtime operations.
- Review and remove unused webhook URLs.
- Confirm branch protection/environments are not left with stale deploy credentials.
- Confirm deploy keys and service identities are revoked or replaced.

## 2) Google / Search Console

- Ensure OAuth client files and token files are no longer usable for this project.
- Revoke Search Console OAuth client and refresh tokens where migration is complete.
- Re-check Google Cloud/API credentials scope and ownership.
- Verify domain verification records are still correct for new ownership/retirement policy.

## 3) Analytics / ads / tracking

- GA4 / Clarity integration: remove/disable scripts if no longer needed.
- Disable ad-related IDs and confirm no runtime ad snippets remain.
- Confirm consent gates / measurement calls are not firing after decommission.

## 4) AI / external APIs

- Revoke/rotate keys for OpenAI or other LLM providers.
- Revoke any image generation/search provider keys in use.
- Confirm no runtime process still uses those keys.

## 5) DNS / TLS / network

- Review DNS records:
  - A / AAAA / CNAME / TXT / MX / other records tied to this project.
- Confirm certificate issuance and binding ownership before final deletion/transfer.
- Confirm firewall rules removed for project traffic paths.
- Document final domain routing end-state.

## 6) Final validation

- Mark this checklist complete only after all items above are verified.
- Keep `DECOMMISSION_REPORT.md` aligned with this checklist for audit evidence.
- Preserve this file for next operator handoff.


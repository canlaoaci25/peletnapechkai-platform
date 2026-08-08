# Evidence-driven scaling and Knowledge Vault

BOECL does not add Redis, a search cluster, queues, object storage, or extra regions merely
for architectural fashion. Consider them only after sustained measurements show a need:
database/search p95 above 500 ms, web p75 LCP above 2.5 s, media storage above 70% disk,
background work affecting request latency, or a recovery objective the current server
cannot meet. Record the measurement and rollback plan before each adoption.

Knowledge Vault begins with source-backed candidates. Every candidate has a locale, claim,
HTTP(S) source, provenance flag, creator, immutable audit trail, and pending/approved/rejected
review state. AI-assisted candidates have no public route and cannot become articles or
published knowledge automatically. An Editor/Admin/Owner must explicitly approve them.

Approved candidates can be linked to same-locale articles as evidence, background, or an
update prompt. Each link carries an editorial note, last verification timestamp, and a
mandatory future review date. Overdue links are highlighted in the admin workspace. These
relationships remain private editorial data and never bypass the article workflow.

Future model-provider integration must keep prompts and credentials outside the database,
redact personal data, enforce cost/rate limits, record model/version provenance, and retain
the same human review boundary.

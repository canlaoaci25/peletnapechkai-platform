# Editorial workflow

Phase 4 starts with authorized API operations under `/api/v1/admin/articles`. Drafts can
be created and edited by `WriteContent`; every saved edit stores the previous title,
summary, and body as an immutable numbered revision.

The workflow is:

`Draft` → `InEditorialReview` → `InSeoReview` → `Scheduled` or `Published` → `Archived`

Reviewers can return content to Draft. Only Draft content can be edited. Scheduling
requires a future UTC timestamp, and only SEO-reviewed or scheduled content can publish.
Every mutation requires CSRF protection, an appropriate role policy, and creates an
append-only audit record. Updates include the previously loaded `UpdatedAt` value and
return `409 Conflict` when another edit won the race.

The first administration UI slice will consume these endpoints through a same-origin
Next.js Backend-for-Frontend route so API cookies remain HTTP-only.

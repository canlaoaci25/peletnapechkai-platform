# Public publishing

Public content is exposed through read-only locale-scoped API endpoints and rendered by
the Next.js application at runtime. Only records in `Published` status are returned;
Draft, review, Scheduled, and Archived content is never included.

- Listing: `GET /api/v1/public/{locale}/articles`
- Article: `GET /api/v1/public/{locale}/articles/{slug}`
- Public route: `/{locale}/articles/{slug}`

The home page lists the latest publications for its exact locale. Article pages return
404 for missing, unpublished, archived, disabled-locale, or cross-locale slugs. Metadata
uses the localized SEO fields when present and always emits a self-referencing canonical
URL.

Article bodies are currently treated as untrusted plain text and rendered as React text
paragraphs. Raw HTML is not accepted or injected. A future structured editor or Markdown
pipeline must add explicit parsing, sanitization, link policy, and tests before enabling
richer output.

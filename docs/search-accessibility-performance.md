# Search, accessibility, and performance

Public search is locale-scoped and includes only Published content. Queries shorter than
two characters return no results. PostgreSQL wildcard characters are escaped before the
case-insensitive title, summary, and body match, and results are capped.

Every locale tree includes a keyboard-visible skip link with a focusable target. Search
uses native GET forms, explicit labels, semantic result lists, live result counts, and
localized copy. Existing focus indicators, heading hierarchy, document language, and
404 behavior remain required quality gates.

Public pages render on demand so new editorial publications are visible without a full
site rebuild. RSS uses a short public cache. `Test-PublicExperience.ps1` validates all
locale homepages, search landmarks, skip links, RSS endpoints, and sitemap output against
staging or production.

The current database search is intentionally simple for the initial corpus. Before the
corpus becomes large, measure query plans and add PostgreSQL full-text/trigram indexes or
a dedicated search service only when the data justifies it.

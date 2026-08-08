# Technical SEO and localization

BOECL emits self-referencing canonicals for locale homepages and published articles.
Article `hreflang` links are generated only from enabled, actually published sibling
localizations in the same article group; missing translations never fall back to another
language's content URL.

Production provides a dynamic `/sitemap.xml`, locale feeds at `/{locale}/feed.xml`, and a
`robots.txt` that blocks administration and API paths. Staging builds disallow all robots,
omit the sitemap directive, and retain the IIS `X-Robots-Tag` defense in depth.

Published article pages include escaped JSON-LD `Article` data with headline, description,
publication/modification dates, language, canonical entity URL, and BOECL publisher name.
RSS text is XML-escaped. Draft, review, scheduled, archived, disabled-locale, and missing
content is excluded from feeds, sitemaps, public APIs, and structured data.

# SEO and International SEO implementation

This project has a locale-aware SEO implementation with explicit public and staging behavior.

## Public metadata model

- `apps/web/src/app/[locale]/layout.tsx` sets locale metadata and alternates:
  - `alternates.canonical = /{locale}`
  - `alternates.languages` includes all enabled locales + `x-default`.
  - `openGraph` and `application/rss+xml` link are configured.
- Article pages set locale-specific canonical and `alternates.languages` from available translations.
- Collection pages (topics, categories, tags, legal, sources, archives) define `canonical` and language alternates via helpers.
- Admin/public account pages set noindex/noarchive robot directives.

## Sitemap and discovery surfaces

- Dynamic XML sitemap: `apps/web/src/app/sitemap.ts`
  - Includes locale home pages.
- Includes article URLs, topic page, categories, tags, author pages, source pages, legal pages.
- Uses locale alternates (`hreflang`) per URL group where translations exist.
- Archive and source pages include language maps where data exists.

- Dynamic plain-text sitemap: `GET /sitemap.txt` (`apps/web/src/app/sitemap.txt/route.ts`)
  - Serves `public/sitemap.txt` generated periodically.
  - Returns `503` if sitemap file missing/empty.

- RSS:
  - `GET /{locale}/feed.xml` serves latest published items for that locale.
  - Description uses site name + locale and escapes XML values.

- Indexing policy:
  - `robots.txt` is locale/staging aware.
  - Staging blocks all crawling (`Disallow: /`) and omits sitemap.
  - Production allows crawl except explicit admin/account/search/api paths.

## Canonical / hreflang behavior

- Canonical URLs are generated for locale routes with explicit path and locale.
- `alternates.languages` for articles points to available published translations.
- `x-default` is mapped to Turkish (`tr-TR`) or fallback from available translations.
- Hreflang is not generated for missing translation content.

## International SEO logic

- Default locale for rendering: `tr-TR`.
- Runtime locale resolution in `apps/web/src/proxy.ts` prioritizes:
  1) explicit locale segment
  2) `boecl-locale` cookie
  3) IP geolocation/country mapping from locale config
  4) browser language
  5) default locale
- Locale directory used by proxy is fetched from backend `/api/v1/locales`.
- Public content endpoints are locale-scoped (`/api/v1/public/{locale}/...`), so language-specific discovery stays aligned to published data.

## Structured data and trust signals

- Article pages emit JSON-LD:
  - `Article` schema with published/updated dates, authors, categories/tags, canonical URL and publisher.
  - Breadcrumb schema for readability and SERP context.
- Commercial disclosure markers are added for sponsored/affiliate content as part of trust UX (not SEO tags).

## Technical SEO checks

Current checks and test coverage include:

- locale-aware canonical/hreflang generation
- noindex/noarchive on restricted pages
- sitemap and feed generation
- search/explore route metadata
- sitemap/robots output for staging vs production

See `apps/web/src/lib/*` tests for regression coverage around:
- `international-experience`
- `topic-discovery`
- `source-center`
- `responsive-images`

## SEO automation and integrations

- Optional integration points exist but are environment-driven:
  - GA4 (`NEXT_PUBLIC_GA_MEASUREMENT_ID`)
  - Microsoft Clarity (`NEXT_PUBLIC_CLARITY_PROJECT_ID`)
  - AdSense (`NEXT_PUBLIC_ADSENSE_CLIENT`)
  - IndexNow (`INDEXNOW_KEY`)
  - Search Console + domain verification metadata (`GOOGLE_SITE_VERIFICATION`, Bing metadata env)
- Production analytics scripts run only after consent handling in the frontend.

## Known limitations / follow-up

- No CDN/object storage layer is configured in-repo.
- API routes and media still depend on local filesystem paths for media.
- Search indexing optimization is currently static by page/component state, not external crawler intelligence.

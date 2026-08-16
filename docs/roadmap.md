# Delivery roadmap

## Execution status

Completed foundations include server/runtime setup, production safeguards, BOECL naming,
isolated staging, identity and editorial administration, public article publishing, and
localized technical SEO. The remaining delivery order is:

1. Search, accessibility, performance, and automated browser checks.
2. Media upload/management plus category, tag, author, and source experiences.
3. Measurement dashboard, legal/editorial pages, consent baseline, staging validation,
   and production promotion are complete. External webmaster/analytics accounts and
   jurisdiction-specific legal review remain launch-owner tasks.
4. Revenue controls, AdSense connection, ads.txt, and commercial disclosures are complete;
   Google site review remains external.
5. Evidence-driven scaling thresholds and the human-reviewed Knowledge Vault foundation
   plus administration workspace are complete.
6. Global/Turkish publication research and the responsive BOECL light, dark, and system
   theme baseline are complete; future iterations should follow measured reader behavior.

Each item remains open until it passes staging, automated checks, GitHub push, and the
relevant production verification. Secrets and unreviewed content are excluded from every
delivery.

## Phase 0 — Decisions and scope

Architecture, initial locales, MVP boundaries, editorial workflow, and success criteria.

## Phase 1 — Repository and development foundation

Next.js web application, ASP.NET Core API, tests, documentation, quality scripts, private
GitHub repository, and CI foundation.

## Phase 2 — Database and domain

PostgreSQL, EF Core migrations, locale/region, article groups and localizations,
categories, tags, authors, sources, media, revisions, SEO metadata, users, and audit logs.

## Phase 3 — Identity and authorization

Secure cookie authentication, owner/admin/editor/author/translator/SEO roles, session
management, login protection, and audit trails.

## Phase 4 — Administration and editorial workflow

Article editor, localization management, review, SEO approval, scheduling, publishing,
preview, media management, and revision comparison.

## Phase 5 — Public publishing experience

Locale homepages, categories, tags, articles, authors, search, archives, navigation,
language switching, accessibility, and legal/editorial pages.

## Phase 6 — Internationalization

Localized routes and slugs, region-aware formatting, translation relationships, missing
localization behavior, user preference, and future RTL readiness.

## Phase 7 — Technical SEO

Canonical and hreflang metadata, JSON-LD, sitemaps, news sitemaps, RSS, robots, preview
indexing controls, redirects, structured data, and Core Web Vitals.

## Phase 8 — Staging and production readiness

Staging hostname, IIS and Windows service deployment, HTTPS, security headers, rate
limiting, backup/restore, monitoring, end-to-end tests, rollback, and controlled migration.

## Phase 9 — Measurement and launch

Search Console, Analytics, Clarity, Bing Webmaster Tools, uptime monitoring, indexing,
content quality, and a controlled multi-region launch.

## Phase 10 — Revenue

Consent management, AdSense readiness, ads.txt, managed placements, affiliate links,
sponsorship disclosure, performance measurement, and verified tax/payment structure.

## Phase 11 — Scaling

Only when measurements justify them: Redis, a dedicated search engine, Cloudflare CDN
and R2, job queues, database separation, centralized logs, and additional regions.

## Phase 12 — AI and knowledge network

Human-reviewed AI assistance, content opportunity analysis, freshness and quality tools,
knowledge graph, knowledge vault, topic hubs, comparisons, and editorial decision support.

## Phase 13 — Global publishing design and theme system

Research current international blogs, global digital publications, Onedio, and leading
Turkish news sites. Compare information hierarchy, navigation, article readability,
discovery surfaces, advertising pressure, mobile behavior, accessibility, and trust
signals. Turn the findings into an original BOECL design system rather than copying any
single publication.

Deliver a responsive public theme with user-selectable light, dark, and system modes;
accessible contrast and focus states; stable design tokens; locale-safe typography; and
consistent homepage, listing, article, search, author, and legal-page patterns. Validate
the result across all supported locales, common viewport sizes, keyboard use, reduced
motion, and Core Web Vitals budgets.

## Phase 14 — Membership, interaction, and return visits

Create durable reader value without restricting public editorial content: account-bound
reading lists, accessible save actions, locale-aware saved-story discovery, and audited
ownership controls. Follow with topic subscriptions and consented notifications only when
delivery infrastructure and preference management are complete.

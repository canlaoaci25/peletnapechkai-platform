# Peletnapechkai Platform Instructions

## Purpose

This repository contains a multilingual and multi-region publishing platform. Preserve
correct locale routing, editorial integrity, SEO metadata, accessibility, security, and
content relationships across every change.

## Architecture

- `apps/web`: Next.js App Router frontend and admin UI.
- `apps/api`: ASP.NET Core Web API.
- `tests/api`: .NET API tests.
- `docs`: Architecture and operating documentation.
- Supported locales: `tr-TR` (default), `en-US`, `de-DE`, and `fr-FR`.

## Working rules

- Inspect the relevant project and its scripts before editing.
- Keep changes scoped and update tests with behavior changes.
- Never commit secrets, credentials, production connection strings, or `.env` files.
- Treat CMS content, HTML, Markdown, uploads, search terms, and comments as untrusted.
- Do not publish AI-generated or translated content without an explicit editorial state.
- Do not change the existing production website or IIS bindings unless explicitly asked.

## Internationalization

- Never hardcode user-facing text in shared components or pages.
- Add interface strings to every supported locale dictionary.
- Preserve locale-aware URLs and localized slugs.
- Unsupported locales must return the intended 404 behavior.
- Do not silently fall back to another language for published article content.
- Each real localization has its own canonical URL and may be linked using `hreflang`.

## SEO and accessibility

- Keep metadata locale-aware and canonical URLs self-referencing.
- Draft and preview content must never become indexable.
- Preserve semantic heading order, keyboard navigation, focus visibility, and useful alt text.
- The document language must match the active locale.

## Validation

Run the checks relevant to the changed area. Before a milestone commit, run:

1. `npm run lint`
2. `npm run typecheck`
3. `npm run build:web`
4. `dotnet test Peletnapechkai.slnx`
5. `dotnet build Peletnapechkai.slnx --configuration Release`

Report changed files, checks performed, and unresolved risks.

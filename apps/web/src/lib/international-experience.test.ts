import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import test from "node:test";

const read = (path: string) => readFileSync(new URL(path, import.meta.url), "utf8");

test("article language menu links only published equivalents and explains missing translations", () => {
  const article = read("../app/[locale]/articles/[slug]/page.tsx");
  const header = read("../components/site-header.tsx");
  assert.match(article, /localeHrefs=\{Object\.fromEntries\(article\.translations/);
  assert.match(header, /localeHrefs && !localeHrefs\[supportedLocale\]/);
  assert.match(header, /aria-disabled="true"/);
  assert.match(header, /translationUnavailable/);
});

test("international publishing dashboard exposes coverage and editorial debt", () => {
  const manager = read("../components/admin/language-manager.tsx");
  assert.match(manager, /missingTranslationCount/);
  assert.match(manager, /reviewPendingCount/);
  assert.match(manager, /sourcePublishedCount/);
  assert.match(manager, /language-health-summary/);
  assert.match(manager, /staleTranslationCount/);
  assert.match(manager, /missingCategoryCount/);
  assert.match(manager, /linkedCategoryCount/);
});

test("localization debt has locale-complete ownership and SLA controls", () => {
  const manager = read("../components/admin/language-manager.tsx");
  const endpoint = read("../../../api/Endpoints/LocaleManagementEndpoints.cs");
  const css = read("../app/globals.css");
  for (const locale of ["tr-TR", "en-US", "de-DE", "fr-FR"]) assert.match(manager, new RegExp(`"${locale}"`));
  assert.match(manager, /LocalizationWorkQueue/);
  assert.match(manager, /assigneeUserId/);
  assert.match(manager, /x-csrf-token/);
  assert.match(endpoint, /localization\.assignment_updated/);
  assert.match(endpoint, /ValidateAntiforgery/);
  assert.ok(css.includes("@media(max-width:600px){.localization-work>header"));
  assert.ok(css.includes(".localization-work-card form{grid-template-columns:1fr}"));
});

test("localized tag archives expose reciprocal locale routes and parity debt", () => {
  const endpoint = read("../../../api/Endpoints/PublicContentEndpoints.cs");
  const manager = read("../components/admin/language-manager.tsx");
  const sitemap = read("../app/sitemap.ts");
  assert.match(endpoint, /SourceTagId/);
  assert.match(endpoint, /tagTranslationKey/);
  assert.match(manager, /missingTagCount/);
  assert.match(sitemap, /tagLanguages/);
});

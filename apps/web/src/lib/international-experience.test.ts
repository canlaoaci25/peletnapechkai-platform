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
});

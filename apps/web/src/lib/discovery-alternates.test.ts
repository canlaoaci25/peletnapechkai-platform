import assert from "node:assert/strict";
import test from "node:test";
import { buildLocaleAlternates } from "./locale-alternates-core.ts";

const locales = ["tr-TR", "en-US", "de-DE", "fr-FR"] as const;

test("static discovery pages expose every supported locale and Turkish x-default", () => {
  const paths = Object.fromEntries(locales.map((locale) => [locale, `/${locale}/sources`]));
  assert.deepEqual(buildLocaleAlternates(locales, "tr-TR", paths), {
    "tr-TR": "/tr-TR/sources",
    "en-US": "/en-US/sources",
    "de-DE": "/de-DE/sources",
    "fr-FR": "/fr-FR/sources",
    "x-default": "/tr-TR/sources",
  });
});

test("dynamic discovery pages never invent a missing translation", () => {
  assert.deepEqual(buildLocaleAlternates(locales, "tr-TR", { "en-US": "/en-US/sources/example.com", "fr-FR": "/fr-FR/sources/example.com" }), {
    "en-US": "/en-US/sources/example.com",
    "fr-FR": "/fr-FR/sources/example.com",
    "x-default": "/en-US/sources/example.com",
  });
});

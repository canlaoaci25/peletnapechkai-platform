import assert from "node:assert/strict";
import { mkdtemp, mkdir, rm, writeFile } from "node:fs/promises";
import os from "node:os";
import path from "node:path";
import test from "node:test";
import { validateLocaleConsistency } from "./check-locales.mjs";

async function fixture(t, translated = { search: { results: "{count} results" } }) {
  const root = await mkdtemp(path.join(os.tmpdir(), "boecl-locales-"));
  t.after(() => rm(root, { recursive: true, force: true }));
  const config = { defaultLocale: "tr-TR", locales: { "tr-TR": "Türkiye — Türkçe", "en-US": "United States — English" } };
  const dictionaryDirectory = path.join(root, "apps/web/src/i18n/dictionaries");
  await mkdir(dictionaryDirectory, { recursive: true });
  await mkdir(path.join(root, "config"), { recursive: true });
  await writeFile(path.join(root, "config/supported-locales.json"), JSON.stringify(config));
  await writeFile(path.join(root, "apps/web/src/i18n/supported-locales.generated.json"), JSON.stringify(config));
  await writeFile(path.join(dictionaryDirectory, "tr-TR.json"), JSON.stringify({ search: { results: "{count} sonuç" } }));
  await writeFile(path.join(dictionaryDirectory, "en-US.json"), JSON.stringify(translated));
  return root;
}

test("accepts complete dictionaries with matching placeholders", async (t) => {
  const root = await fixture(t);
  assert.deepEqual(await validateLocaleConsistency(root), ["tr-TR", "en-US"]);
});

test("rejects a missing translation key", async (t) => {
  const root = await fixture(t, { search: {} });
  await assert.rejects(validateLocaleConsistency(root), /en-US dictionary mismatch\. Missing: search\.results/);
});

test("rejects a blank translation", async (t) => {
  const root = await fixture(t, { search: { results: "  " } });
  await assert.rejects(validateLocaleConsistency(root), /en-US dictionary value search\.results must not be blank/);
});

test("rejects a non-string translation leaf", async (t) => {
  const root = await fixture(t, { search: { results: 12 } });
  await assert.rejects(validateLocaleConsistency(root), /en-US dictionary value search\.results must be a string/);
});

test("rejects missing or renamed placeholders", async (t) => {
  const root = await fixture(t, { search: { results: "{total} results" } });
  await assert.rejects(validateLocaleConsistency(root), /placeholder mismatch at search\.results\. Expected: \{count\}; actual: \{total\}/);
});

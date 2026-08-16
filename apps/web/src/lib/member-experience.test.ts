import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import test from "node:test";
import { fileURLToPath } from "node:url";
import { memberCopy } from "../i18n/member-copy.ts";

test("member reading-list copy covers every supported locale", () => {
  assert.deepEqual(Object.keys(memberCopy).sort(), ["de-DE", "en-US", "fr-FR", "tr-TR"]);
  for (const copy of Object.values(memberCopy)) {
    assert.ok(copy.savedTitle.length > 0);
    assert.ok(copy.signInToSave.length > 0);
    assert.ok(copy.savedEmpty.length > 0);
  }
});

test("article save control is a csrf-protected accessible toggle", () => {
  const source = readFileSync(fileURLToPath(new URL("../components/save-article-button.tsx", import.meta.url)), "utf8");
  assert.match(source, /<button[^>]+aria-pressed=\{saved\}/);
  assert.match(source, /"x-csrf-token":token/);
  assert.match(source, /method:saved\?"DELETE":"PUT"/);
  assert.match(source, /role="status"/);
});

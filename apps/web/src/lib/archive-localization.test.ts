import assert from "node:assert/strict";
import test from "node:test";
import { archiveLanguages } from "./archive-localization.ts";

test("links translated categories and uses Turkish as x-default", () => {
  assert.deepEqual(archiveLanguages("categories", [
    { locale: "en-US", slug: "science" },
    { locale: "tr-TR", slug: "bilim" },
    { locale: "de-DE", slug: "wissenschaft" },
  ]), {
    "en-US": "/en-US/categories/science",
    "tr-TR": "/tr-TR/categories/bilim",
    "de-DE": "/de-DE/categories/wissenschaft",
    "x-default": "/tr-TR/categories/bilim",
  });
});

test("does not infer translation relationships for unlinked taxonomy", () => {
  assert.equal(archiveLanguages("tags", [{ locale: "en-US", slug: "ai" }]), undefined);
  assert.equal(archiveLanguages("categories", []), undefined);
});

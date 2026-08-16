import assert from "node:assert/strict";
import test from "node:test";
import { estimateReadingMinutes, wasMeaningfullyUpdated } from "./article-reading.ts";

test("HTML işaretlerini ve betik içeriğini okuma süresine katmaz", () => {
  const words = Array.from({ length: 221 }, () => "kelime").join(" ");
  assert.equal(estimateReadingMinutes(`<p>${words}</p><script>${"gizli ".repeat(500)}</script>`), 2);
});

test("boş ve kısa içerikler için en az bir dakika döndürür", () => {
  assert.equal(estimateReadingMinutes(""), 1);
  assert.equal(estimateReadingMinutes("Kısa bir yazı."), 1);
});

test("yalnızca anlamlı güncelleme farkını işaretler", () => {
  assert.equal(wasMeaningfullyUpdated("2026-08-16T08:00:00Z", "2026-08-16T08:30:00Z"), false);
  assert.equal(wasMeaningfullyUpdated("2026-08-16T08:00:00Z", "2026-08-16T10:00:00Z"), true);
});

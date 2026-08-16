import assert from "node:assert/strict";
import test from "node:test";
import { buildArticleOutline } from "./article-outline.ts";

test("builds an accessible outline and preserves safe heading markup", () => {
  const result = buildArticleOutline("<p>Giriş</p><h2>Mobil &amp; erişilebilirlik</h2><h3><strong>Hızlı</strong> başlangıç</h3>");
  assert.deepEqual(result.outline, [
    { id: "mobil-erisilebilirlik", label: "Mobil & erişilebilirlik", level: 2 },
    { id: "hizli-baslangic", label: "Hızlı başlangıç", level: 3 },
  ]);
  assert.match(result.bodyHtml, /<h2 id="mobil-erisilebilirlik" tabindex="-1">Mobil &amp; erişilebilirlik<\/h2>/);
  assert.match(result.bodyHtml, /<h3 id="hizli-baslangic" tabindex="-1"><strong>Hızlı<\/strong> başlangıç<\/h3>/);
});

test("creates stable unique ids for repeated and non-latin headings", () => {
  const result = buildArticleOutline("<h2>Özet</h2><h2>Özet</h2><h3>人工知能</h3>");
  assert.deepEqual(result.outline.map(({ id }) => id), ["ozet", "ozet-2", "section-3"]);
});

test("keeps sanitizer-approved heading attributes", () => {
  const result = buildArticleOutline('<h2 class="section-title">Performans</h2>');
  assert.equal(result.bodyHtml, '<h2 class="section-title" id="performans" tabindex="-1">Performans</h2>');
});

test("does not mistake other elements or empty headings for outline entries", () => {
  const html = "<h4>Detay</h4><p>&lt;h2&gt;metin&lt;/h2&gt;</p><h2> </h2>";
  assert.deepEqual(buildArticleOutline(html), { bodyHtml: html, outline: [] });
});

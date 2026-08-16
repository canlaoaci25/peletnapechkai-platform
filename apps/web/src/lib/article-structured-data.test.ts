import assert from "node:assert/strict";
import test from "node:test";
import { buildArticleStructuredData } from "./article-structured-data.ts";

const base = {
  title: "Kaynaklı Türkçe teknoloji incelemesi",
  summary: "Okur özeti",
  seoDescription: "Arama motoruna özel Türkçe açıklama",
  publishedAt: "2026-08-16T08:00:00Z",
  updatedAt: "2026-08-16T09:00:00Z",
  locale: "tr-TR",
  canonicalUrl: "https://boecl.com/tr-TR/articles/kaynakli-inceleme",
  categories: [{ name: "Teknoloji" }, { name: "Teknoloji" }],
  tags: [{ name: "Yapay zekâ" }, { name: "Güvenlik" }],
  authors: [{ displayName: "BOECL Editör", url: "https://boecl.com/tr-TR/authors/boecl" }],
  sources: [
    { url: "https://example.org/research" },
    { url: "https://example.org/research" },
    { url: "javascript:alert(1)" },
  ],
  publisher: { id: "https://boecl.com/#organization", name: "BOECL", url: "https://boecl.com/tr-TR" },
};

test("kaynakları ve taxonomy alanlarını Article şemasına güvenle taşır", () => {
  const result = buildArticleStructuredData(base);

  assert.equal(result.description, base.seoDescription);
  assert.deepEqual(result.citation, ["https://example.org/research"]);
  assert.deepEqual(result.articleSection, ["Teknoloji"]);
  assert.deepEqual(result.keywords, ["Yapay zekâ", "Güvenlik"]);
  assert.equal(result.inLanguage, "tr-TR");
});

test("boş SEO alanlarında özet kullanır ve boş dizileri yayımlamaz", () => {
  const result = buildArticleStructuredData({
    ...base,
    seoDescription: null,
    categories: [],
    tags: [],
    sources: [{ url: "file:///private/source.txt" }],
  });

  assert.equal(result.description, base.summary);
  assert.equal(result.citation, undefined);
  assert.equal(result.articleSection, undefined);
  assert.equal(result.keywords, undefined);
});

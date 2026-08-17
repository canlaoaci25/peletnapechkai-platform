import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import test from "node:test";

const indexPage=readFileSync(new URL("../app/[locale]/sources/page.tsx",import.meta.url),"utf8");
const archivePage=readFileSync(new URL("../app/[locale]/sources/[domain]/page.tsx",import.meta.url),"utf8");
const articlePage=readFileSync(new URL("../app/[locale]/articles/[slug]/page.tsx",import.meta.url),"utf8");
const sitemap=readFileSync(new URL("../app/sitemap.ts",import.meta.url),"utf8");
const copy=readFileSync(new URL("../i18n/source-copy.ts",import.meta.url),"utf8");

test("source center is localized and exposes evidence-led archives",()=>{
  for(const locale of ["tr-TR","en-US","de-DE","fr-FR"]) assert.match(copy,new RegExp(`"${locale}"`));
  assert.match(indexPage,/totalSources/);
  assert.match(indexPage,/citationCount/);
  assert.match(archivePage,/archiveDescription/);
  assert.match(archivePage,/article\.cover/);
});

test("source archives are connected to articles and search discovery",()=>{
  assert.match(articlePage,/sources\/\$\{source\.host\.replace/);
  assert.match(sitemap,/getPublicSourceIndex/);
  assert.match(sitemap,/latestCitationAt/);
});

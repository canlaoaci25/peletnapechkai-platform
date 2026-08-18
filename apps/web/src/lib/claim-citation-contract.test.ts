import assert from "node:assert/strict";
import test from "node:test";
import { readFileSync } from "node:fs";

const page=readFileSync(new URL("../app/[locale]/articles/[slug]/page.tsx",import.meta.url),"utf8");
const manager=readFileSync(new URL("../components/admin/claim-citation-manager.tsx",import.meta.url),"utf8");
const copy=readFileSync(new URL("../i18n/claim-citation-copy.ts",import.meta.url),"utf8");

test("claim evidence is editor-managed, locale-complete and visible to readers",()=>{
  for(const locale of ["tr-TR","en-US","de-DE","fr-FR"])assert.match(copy,new RegExp(`"${locale}"`));
  assert.match(manager,/x-csrf-token/);assert.match(manager,/sourceIds\.includes/);
  assert.match(page,/article\.claimCitations\.length>0/);assert.match(page,/aria-labelledby="claim-citations-title"/);
  assert.match(page,/nofollow noopener noreferrer/);assert.match(page,/item\.locator/);
});

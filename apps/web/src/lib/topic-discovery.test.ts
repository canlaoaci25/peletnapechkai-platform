import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import test from "node:test";

const page = readFileSync(new URL("../app/[locale]/topics/page.tsx", import.meta.url), "utf8");
const archive = readFileSync(new URL("../app/[locale]/[collection]/[slug]/page.tsx", import.meta.url), "utf8");
const migration = readFileSync(new URL("../../../api/Infrastructure/Persistence/Migrations/20260816190000_AddMobileTechnologyTaxonomy.cs", import.meta.url), "utf8");
const proxy = readFileSync(new URL("../proxy.ts", import.meta.url), "utf8");

test("konu merkezi locale-aware canonical, hreflang ve gerçek yayın kapakları sunar", () => {
  assert.ok(page.includes('canonical: `/${locale}/topics`'));
  assert.match(page, /languages: Object\.fromEntries/);
  assert.match(page, /category\.articleCount/);
  assert.match(page, /category\.featured\.find/);
  assert.match(page, /alt=""/);
});

test("kategori arşivi yayın kapağını yinelenen klavye durağı oluşturmadan gösterir", () => {
  assert.match(archive, /archive-card-cover/);
  assert.match(archive, /tabIndex=\{-1\}/);
  assert.match(archive, /aria-hidden="true"/);
});

test("mobil taxonomy migrationı dört locale, idempotency, audit ve rollback içerir", () => {
  for (const locale of ["tr-TR", "en-US", "de-DE", "fr-FR"]) assert.match(migration, new RegExp(locale));
  assert.match(migration, /ON CONFLICT DO NOTHING/);
  assert.match(migration, /migration\.mobile_technology_taxonomy_added/);
  assert.match(migration, /protected override void Down/);
});

test("URL'de açıkça seçilen locale ülke sinyaliyle değiştirilmez", () => {
  const explicitLocaleBranch = proxy.slice(proxy.indexOf("if (firstSegment && hasLocale(firstSegment))"), proxy.indexOf("const directory = await localeDirectory();", proxy.indexOf("if (firstSegment && hasLocale(firstSegment))")));
  assert.doesNotMatch(explicitLocaleBranch, /NextResponse\.redirect/);
  assert.match(explicitLocaleBranch, /NextResponse\.next/);
});

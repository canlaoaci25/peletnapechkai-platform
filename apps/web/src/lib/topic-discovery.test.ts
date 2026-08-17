import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import test from "node:test";

const page = readFileSync(new URL("../app/[locale]/topics/page.tsx", import.meta.url), "utf8");
const archive = readFileSync(new URL("../app/[locale]/[collection]/[slug]/page.tsx", import.meta.url), "utf8");
const migration = readFileSync(new URL("../../../api/Infrastructure/Persistence/Migrations/20260816190000_AddMobileTechnologyTaxonomy.cs", import.meta.url), "utf8");
const softwareMigration = readFileSync(new URL("../../../api/Infrastructure/Persistence/Migrations/20260816203000_AddSoftwareApplicationsTaxonomy.cs", import.meta.url), "utf8");
const privacyMigration = readFileSync(new URL("../../../api/Infrastructure/Persistence/Migrations/20260816220000_AddPrivacyDigitalRightsTaxonomy.cs", import.meta.url), "utf8");
const smartHomeMigration = readFileSync(new URL("../../../api/Infrastructure/Persistence/Migrations/20260817030000_AddSmartHomeConnectedLivingTaxonomy.cs", import.meta.url), "utf8");
const hierarchyMigration = readFileSync(new URL("../../../api/Infrastructure/Persistence/Migrations/20260817042309_AddCategoryHierarchyAndMobilityTaxonomy.cs", import.meta.url), "utf8");
const knowledgeMigration = readFileSync(new URL("../../../api/Infrastructure/Persistence/Migrations/20260817124500_AddKnowledgeManagementNotesTaxonomy.cs", import.meta.url), "utf8");
const parityMigration = readFileSync(new URL("../../../api/Infrastructure/Persistence/Migrations/20260817043000_RepairMobilityTaxonomyLocaleParity.cs", import.meta.url), "utf8");
const publicApi = readFileSync(new URL("../../../api/Endpoints/PublicContentEndpoints.cs", import.meta.url), "utf8");
const supportingApi = readFileSync(new URL("../../../api/Endpoints/SupportingContentEndpoints.cs", import.meta.url), "utf8");
const proxy = readFileSync(new URL("../proxy.ts", import.meta.url), "utf8");

test("konu merkezi locale-aware canonical, hreflang ve gerçek yayın kapakları sunar", () => {
  assert.ok(page.includes('canonical: `/${locale}/topics`'));
  assert.match(page, /languages: Object\.fromEntries/);
  assert.match(page, /category\.articleCount/);
  assert.match(page, /category\.featured\.find/);
  assert.match(page, /alt=""/);
});

test("konu merkezi öne çıkan konu ve doğrudan makale keşif yolları sunar",()=>{
  assert.match(page,/topic-lead/);
  assert.match(page,/lead\.featured\.map/);
  assert.match(page,/category\.featured\.slice\(0,2\)/);
  assert.ok(page.includes('href={`/${locale}/articles/${article.slug}`}'));
  assert.doesNotMatch(publicApi,/foreach \(var item in categoryRows\)/);
});

test("konu merkezi parent-child keşif yollarını gerçek arşiv ilişkilerinden kurar",()=>{
  assert.match(page,/filter\(category => !category\.parent\)/);
  assert.match(page,/category\.children\.map/);
  assert.match(page,/topic-children/);
  assert.match(publicApi,/item\.ParentCategory/);
  assert.match(publicApi,/item\.Children/);
});

test("mobilite taxonomy migrationı dört locale, hiyerarşi, audit ve güvenli rollback içerir",()=>{
  for(const locale of ["tr-TR","en-US","de-DE","fr-FR"]) assert.match(hierarchyMigration,new RegExp(locale));
  assert.match(hierarchyMigration,/Otomobil Teknolojileri ve Mobilite/);
  assert.match(hierarchyMigration,/parent_category_id/);
  assert.match(hierarchyMigration,/ON CONFLICT DO NOTHING/);
  assert.match(hierarchyMigration,/migration\.category_hierarchy_mobility_added/);
  assert.match(hierarchyMigration,/protected override void Down/);
  assert.match(hierarchyMigration,/LEFT JOIN categories AS parent/);
  assert.match(parityMigration,/ON CONFLICT \(locale_id, slug\) DO NOTHING/);
  assert.match(parityMigration,/mobility_taxonomy_locale_parity_repaired/);
});

test("bilgi yönetimi taxonomy migrationı dört locale, üst konu, audit ve güvenli rollback içerir",()=>{
  assert.match(knowledgeMigration,/bilgi-yonetimi-ve-not-alma/);
  assert.match(knowledgeMigration,/knowledge-management-and-note-taking/);
  assert.match(knowledgeMigration,/wissensmanagement-und-notizen/);
  assert.match(knowledgeMigration,/gestion-des-connaissances-et-prise-de-notes/);
  assert.match(knowledgeMigration,/parent_category_id/);
  assert.match(knowledgeMigration,/migration\.knowledge_management_notes_taxonomy_added/);
  assert.match(knowledgeMigration,/ON CONFLICT DO NOTHING/);
  assert.match(knowledgeMigration,/protected override void Down/);
});

test("akıllı ev taxonomy migrationı dört locale, audit, idempotency ve rollback içerir",()=>{
  for(const locale of ["tr-TR","en-US","de-DE","fr-FR"]) assert.match(smartHomeMigration,new RegExp(locale));
  assert.match(smartHomeMigration,/ON CONFLICT DO NOTHING/);
  assert.match(smartHomeMigration,/migration\.smart_home_connected_living_taxonomy_added/);
  assert.match(smartHomeMigration,/article_group_id/);
  assert.match(smartHomeMigration,/protected override void Down/);
});

test("kategori arşivi yayın kapağını yinelenen klavye durağı oluşturmadan gösterir", () => {
  assert.match(archive, /archive-card-cover/);
  assert.match(archive, /tabIndex=\{-1\}/);
  assert.match(archive, /aria-hidden="true"/);
});

test("kategori otorite merkezi derinlik, tür dağılımı ve ilişkili konu yolları sunar", () => {
  assert.match(archive, /archive-authority-hero/);
  assert.match(archive, /archive\.articleCount/);
  assert.match(archive, /archive\.typeCounts/);
  assert.match(archive, /archive\.relatedCategories/);
  assert.match(publicApi, /GroupBy\(article => article\.ArticleGroup\.Type\)/);
  assert.match(publicApi, /SelectMany\(article => article\.Categories\)/);
  assert.match(publicApi, /parent/);
  assert.match(archive, /archive\.parent/);
  assert.match(archive, /BreadcrumbList/);
});

test("kategori arşivi kararlı, canonical sayfalama ve ileri geri keşif yolları sunar",()=>{
  assert.match(archive,/searchParams/);
  assert.match(archive,/archive\.totalPages/);
  assert.match(archive,/rel="prev"/);
  assert.match(archive,/rel="next"/);
  assert.match(publicApi,/Skip\(\(currentPage - 1\) \* take\)/);
  assert.match(publicApi,/ThenBy\(article => article\.Id\)/);
});

test("admin taxonomy masası yayın kapsamı ve kategorisiz kuyruğunu gerçek veriden ölçer",()=>{
  assert.match(supportingApi,/uncategorizedCount/);
  assert.match(supportingApi,/!article\.Categories\.Any\(\)/);
  assert.match(supportingApi,/publishedCount/);
});

test("gizlilik taxonomy migrationı dört locale, audit, idempotency ve rollback içerir",()=>{
  for(const locale of ["tr-TR","en-US","de-DE","fr-FR"]) assert.match(privacyMigration,new RegExp(locale));
  assert.match(privacyMigration,/ON CONFLICT DO NOTHING/);
  assert.match(privacyMigration,/migration\.privacy_digital_rights_taxonomy_added/);
  assert.match(privacyMigration,/article_group_id/);
  assert.match(privacyMigration,/protected override void Down/);
});

test("yazılım taxonomy migrationı dört locale, denetlenebilir ilişki ve rollback içerir", () => {
  for (const locale of ["tr-TR", "en-US", "de-DE", "fr-FR"]) assert.match(softwareMigration, new RegExp(locale));
  assert.match(softwareMigration, /ON CONFLICT DO NOTHING/);
  assert.match(softwareMigration, /migration\.software_applications_taxonomy_added/);
  assert.match(softwareMigration, /article_group_id/);
  assert.match(softwareMigration, /protected override void Down/);
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

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations;

[DbContext(typeof(PublishingDbContext))]
[Migration("20260817124500_AddKnowledgeManagementNotesTaxonomy")]
public partial class AddKnowledgeManagementNotesTaxonomy : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        INSERT INTO categories (id, locale_id, source_category_id, parent_category_id, slug, name, description, created_at)
        SELECT spec.id, locale.id, spec.source_id, parent.id, spec.slug, spec.name, spec.description, NOW()
        FROM (VALUES
          ('01a1e9c0-0000-7000-8000-000000000001'::uuid, 'tr-TR', NULL::uuid, 'verimlilik', 'bilgi-yonetimi-ve-not-alma', 'Bilgi Yönetimi ve Not Alma', 'Not alma, kişisel bilgi yönetimi, araştırma, okuma arşivi ve bağlantılı düşünme araçları için kaynaklı incelemeler ve uygulamalı rehberler.'),
          ('01a1e9c0-0000-7000-8000-000000000002'::uuid, 'en-US', '01a1e9c0-0000-7000-8000-000000000001'::uuid, 'productivity', 'knowledge-management-and-note-taking', 'Knowledge Management and Note-Taking', 'Evidence-led reviews and practical guides for note-taking, personal knowledge management, research, reading archives, and connected thinking.'),
          ('01a1e9c0-0000-7000-8000-000000000003'::uuid, 'de-DE', '01a1e9c0-0000-7000-8000-000000000001'::uuid, 'produktivitaet', 'wissensmanagement-und-notizen', 'Wissensmanagement und Notizen', 'Fundierte Tests und Praxisratgeber zu Notizen, persönlichem Wissensmanagement, Recherche, Lese-Archiven und vernetztem Denken.'),
          ('01a1e9c0-0000-7000-8000-000000000004'::uuid, 'fr-FR', '01a1e9c0-0000-7000-8000-000000000001'::uuid, 'productivite', 'gestion-des-connaissances-et-prise-de-notes', 'Gestion des connaissances et prise de notes', 'Tests documentés et guides pratiques sur la prise de notes, la gestion des connaissances, la recherche, les archives de lecture et la pensée connectée.')
        ) AS spec(id, locale_code, source_id, parent_slug, slug, name, description)
        JOIN locales AS locale ON locale.code = spec.locale_code
        JOIN categories AS parent ON parent.locale_id = locale.id AND parent.slug = spec.parent_slug
        ON CONFLICT (locale_id, slug) DO NOTHING;

        WITH knowledge_groups AS (
          SELECT article_group_id FROM article_localizations
          WHERE locale_id = (SELECT id FROM locales WHERE code = 'tr-TR')
            AND slug ~ '(not-alma|bilgi-yonetimi|bilgi-grafi|not-ve-bilgi|dijital-gunluk|okuma-ve-alinti|kaynak-temelli-arastirma|obsidian|anytype|capacities|heptabase|reflect-notes|noteplan|mem-ai|tana-|craft-belge|day-one|readwise-reader)'
        )
        INSERT INTO article_categories (article_localization_id, category_id)
        SELECT article.id, category.id FROM article_localizations AS article
        JOIN knowledge_groups ON knowledge_groups.article_group_id = article.article_group_id
        JOIN locales AS locale ON locale.id = article.locale_id
        JOIN categories AS category ON category.locale_id = locale.id AND category.slug = CASE locale.code
          WHEN 'tr-TR' THEN 'bilgi-yonetimi-ve-not-alma' WHEN 'en-US' THEN 'knowledge-management-and-note-taking'
          WHEN 'de-DE' THEN 'wissensmanagement-und-notizen' WHEN 'fr-FR' THEN 'gestion-des-connaissances-et-prise-de-notes' END
        ON CONFLICT DO NOTHING;

        INSERT INTO audit_logs (id, actor_user_id, action, entity_type, entity_id, details_json, occurred_at)
        SELECT gen_random_uuid(), NULL, 'migration.knowledge_management_notes_taxonomy_added', 'Category', id,
          jsonb_build_object('slug', slug, 'parentCategoryId', parent_category_id), NOW()
        FROM categories WHERE id IN ('01a1e9c0-0000-7000-8000-000000000001','01a1e9c0-0000-7000-8000-000000000002','01a1e9c0-0000-7000-8000-000000000003','01a1e9c0-0000-7000-8000-000000000004');
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DELETE FROM categories WHERE id IN ('01a1e9c0-0000-7000-8000-000000000004','01a1e9c0-0000-7000-8000-000000000003','01a1e9c0-0000-7000-8000-000000000002');
        DELETE FROM categories WHERE id = '01a1e9c0-0000-7000-8000-000000000001';
        """);
}

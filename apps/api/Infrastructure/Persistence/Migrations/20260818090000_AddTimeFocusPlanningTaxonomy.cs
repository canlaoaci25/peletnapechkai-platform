using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations;

[DbContext(typeof(PublishingDbContext))]
[Migration("20260818090000_AddTimeFocusPlanningTaxonomy")]
public partial class AddTimeFocusPlanningTaxonomy : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        INSERT INTO categories (id, locale_id, source_category_id, parent_category_id, slug, name, description, created_at)
        SELECT spec.id, locale.id, spec.source_id, parent.id, spec.slug, spec.name, spec.description, NOW()
        FROM (VALUES
          ('01a2a740-0000-7000-8000-000000000001'::uuid, 'tr-TR', NULL::uuid, 'verimlilik', 'zaman-odak-ve-planlama', 'Zaman, Odak ve Planlama', 'Zamanı planlama, dikkat dağıtıcıları azaltma, odak rutinleri kurma ve takvim ile görev akışlarını birlikte yönetme üzerine uygulamalı rehberler ve kaynaklı incelemeler.'),
          ('01a2a740-0000-7000-8000-000000000002'::uuid, 'en-US', '01a2a740-0000-7000-8000-000000000001'::uuid, 'productivity', 'time-focus-and-planning', 'Time, Focus and Planning', 'Practical guides and evidence-led reviews for planning time, reducing distractions, building focus routines, and coordinating calendars with task workflows.'),
          ('01a2a740-0000-7000-8000-000000000003'::uuid, 'de-DE', '01a2a740-0000-7000-8000-000000000001'::uuid, 'produktivitaet', 'zeit-fokus-und-planung', 'Zeit, Fokus und Planung', 'Praxisratgeber und fundierte Tests für Zeitplanung, weniger Ablenkung, fokussierte Routinen und die gemeinsame Organisation von Kalendern und Aufgaben.'),
          ('01a2a740-0000-7000-8000-000000000004'::uuid, 'fr-FR', '01a2a740-0000-7000-8000-000000000001'::uuid, 'productivite', 'temps-concentration-et-planification', 'Temps, concentration et planification', 'Guides pratiques et tests documentés pour planifier son temps, réduire les distractions, créer des routines de concentration et coordonner calendriers et tâches.')
        ) AS spec(id, locale_code, source_id, parent_slug, slug, name, description)
        JOIN locales AS locale ON locale.code = spec.locale_code
        JOIN categories AS parent ON parent.locale_id = locale.id AND parent.slug = spec.parent_slug
        ON CONFLICT (locale_id, slug) DO NOTHING;

        DO $$
        BEGIN
          IF (SELECT COUNT(*) FROM categories WHERE id IN
            ('01a2a740-0000-7000-8000-000000000001','01a2a740-0000-7000-8000-000000000002','01a2a740-0000-7000-8000-000000000003','01a2a740-0000-7000-8000-000000000004')) <> 4 THEN
            RAISE EXCEPTION 'Time, focus and planning taxonomy conflicts with an existing locale slug.';
          END IF;
        END $$;

        WITH focus_groups AS (
          SELECT article_group_id FROM article_localizations
          WHERE locale_id = (SELECT id FROM locales WHERE code = 'tr-TR')
            AND slug ~ '(akiflow-|amazing-marvin-|clockify-|cold-turkey-|endel-|fantastical-|flowsavvy-|focusmate-|forest-|freedom-|llama-life-|mikro-mola-|morgen-|motion-yapay-zeka-takvim|notion-calendar-|reclaim-ai-|rescuetime-|rize-|routine-gorev|sunsama-|things-3-|ticktick-|todoist-|toggl-track-|toplantisiz-odak|vimcal-)'
        )
        INSERT INTO article_categories (article_localization_id, category_id)
        SELECT article.id, category.id FROM article_localizations AS article
        JOIN focus_groups ON focus_groups.article_group_id = article.article_group_id
        JOIN locales AS locale ON locale.id = article.locale_id
        JOIN categories AS category ON category.locale_id = locale.id AND category.slug = CASE locale.code
          WHEN 'tr-TR' THEN 'zaman-odak-ve-planlama'
          WHEN 'en-US' THEN 'time-focus-and-planning'
          WHEN 'de-DE' THEN 'zeit-fokus-und-planung'
          WHEN 'fr-FR' THEN 'temps-concentration-et-planification' END
        ON CONFLICT DO NOTHING;

        INSERT INTO audit_logs (id, actor_user_id, action, entity_type, entity_id, details_json, occurred_at)
        SELECT gen_random_uuid(), NULL, 'migration.time_focus_planning_taxonomy_added', 'Category', id,
          jsonb_build_object('slug', slug, 'parentCategoryId', parent_category_id), NOW()
        FROM categories WHERE id IN ('01a2a740-0000-7000-8000-000000000001','01a2a740-0000-7000-8000-000000000002','01a2a740-0000-7000-8000-000000000003','01a2a740-0000-7000-8000-000000000004');
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DELETE FROM categories WHERE id IN ('01a2a740-0000-7000-8000-000000000004','01a2a740-0000-7000-8000-000000000003','01a2a740-0000-7000-8000-000000000002');
        DELETE FROM categories WHERE id = '01a2a740-0000-7000-8000-000000000001';
        """);
}

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations;

[DbContext(typeof(PublishingDbContext))]
[Migration("20260818090000_AddTimeFocusPlanningTaxonomy")]
public partial class AddTimeFocusPlanningTaxonomy : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        INSERT INTO categories (id, locale_id, source_category_id, parent_category_id, slug, name, description, created_at)
        SELECT '01a2a740-0000-7000-8000-000000000001', locale.id, NULL, parent.id,
          'zaman-odak-ve-planlama', 'Zaman, Odak ve Planlama',
          'Zamanı planlama, dikkat dağıtıcıları azaltma, odak rutinleri kurma ve takvim ile görev akışlarını birlikte yönetme üzerine uygulamalı rehberler ve kaynaklı incelemeler.', NOW()
        FROM locales AS locale
        JOIN categories AS parent ON parent.locale_id = locale.id AND parent.slug = 'verimlilik'
        WHERE locale.code = 'tr-TR'
        ON CONFLICT (locale_id, slug) DO NOTHING;

        INSERT INTO categories (id, locale_id, source_category_id, parent_category_id, slug, name, description, created_at)
        SELECT spec.id, locale.id, source.id, parent.id, spec.slug, spec.name, spec.description, NOW()
        FROM (VALUES
          ('01a2a740-0000-7000-8000-000000000002'::uuid, 'en-US', 'productivity', 'time-focus-and-planning', 'Time, Focus and Planning', 'Practical guides and evidence-led reviews for planning time, reducing distractions, building focus routines, and coordinating calendars with task workflows.'),
          ('01a2a740-0000-7000-8000-000000000003'::uuid, 'de-DE', 'produktivitaet', 'zeit-fokus-und-planung', 'Zeit, Fokus und Planung', 'Praxisratgeber und fundierte Tests für Zeitplanung, weniger Ablenkung, fokussierte Routinen und die gemeinsame Organisation von Kalendern und Aufgaben.'),
          ('01a2a740-0000-7000-8000-000000000004'::uuid, 'fr-FR', 'productivite', 'temps-concentration-et-planification', 'Temps, concentration et planification', 'Guides pratiques et tests documentés pour planifier son temps, réduire les distractions, créer des routines de concentration et coordonner calendriers et tâches.')
        ) AS spec(id, locale_code, parent_slug, slug, name, description)
        JOIN locales AS locale ON locale.code = spec.locale_code
        LEFT JOIN categories AS parent ON parent.locale_id = locale.id AND parent.slug = spec.parent_slug
        JOIN categories AS source ON source.locale_id = (SELECT id FROM locales WHERE code = 'tr-TR') AND source.slug = 'zaman-odak-ve-planlama'
        ON CONFLICT (locale_id, slug) DO UPDATE SET source_category_id = EXCLUDED.source_category_id;

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
          WHEN 'de-DE' THEN 'zeit-fokus-und-planlama'
          WHEN 'fr-FR' THEN 'temps-concentration-et-planification' END
        ON CONFLICT DO NOTHING;

        INSERT INTO audit_logs (id, actor_user_id, action, entity_type, entity_id, details_json, occurred_at)
        SELECT gen_random_uuid(), NULL, 'migration.time_focus_planning_taxonomy_added', 'Category', category.id,
          jsonb_build_object('slug', category.slug, 'parentCategoryId', category.parent_category_id), NOW()
        FROM categories category JOIN locales locale ON locale.id = category.locale_id
        WHERE (locale.code, category.slug) IN (('tr-TR','zaman-odak-ve-planlama'),('en-US','time-focus-and-planning'),('de-DE','zeit-fokus-und-planlama'),('fr-FR','temps-concentration-et-planification'));
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DELETE FROM categories WHERE id IN ('01a2a740-0000-7000-8000-000000000004','01a2a740-0000-7000-8000-000000000003','01a2a740-0000-7000-8000-000000000002');
        DELETE FROM categories WHERE id = '01a2a740-0000-7000-8000-000000000001';
        """);
}

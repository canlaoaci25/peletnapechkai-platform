using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations;

/// <summary>Adds the localized smart-home topic proven by the Turkish published archive.</summary>
[DbContext(typeof(PublishingDbContext))]
[Migration("20260817030000_AddSmartHomeConnectedLivingTaxonomy")]
public partial class AddSmartHomeConnectedLivingTaxonomy : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        INSERT INTO categories (id, locale_id, source_category_id, slug, name, description, created_at)
        SELECT spec.id, locale.id, spec.source_id, spec.slug, spec.name, spec.description, NOW()
        FROM (VALUES
          ('019c1f42-0000-7000-8000-000000000001'::uuid, 'tr-TR', NULL::uuid, 'akilli-ev-ve-baglantili-yasam', 'Akıllı Ev ve Bağlantılı Yaşam', 'Akıllı ev cihazları, ev ağları, bağlantılı yaşam güvenliği, veri gizliliği ve dayanıklı dijital ev altyapısı için uygulamalı rehberler ve incelemeler.'),
          ('019c1f42-0000-7000-8000-000000000002'::uuid, 'en-US', '019c1f42-0000-7000-8000-000000000001'::uuid, 'smart-home-and-connected-living', 'Smart Home and Connected Living', 'Practical guides and reviews for smart-home devices, home networks, connected-living security, privacy, and resilient digital-home infrastructure.'),
          ('019c1f42-0000-7000-8000-000000000003'::uuid, 'de-DE', '019c1f42-0000-7000-8000-000000000001'::uuid, 'smart-home-und-vernetztes-leben', 'Smart Home und vernetztes Leben', 'Praxisnahe Ratgeber und Tests zu Smart-Home-Geräten, Heimnetzen, Sicherheit, Datenschutz und einer robusten digitalen Infrastruktur zu Hause.'),
          ('019c1f42-0000-7000-8000-000000000004'::uuid, 'fr-FR', '019c1f42-0000-7000-8000-000000000001'::uuid, 'maison-intelligente-et-vie-connectee', 'Maison intelligente et vie connectée', 'Guides pratiques et essais sur les appareils domestiques connectés, les réseaux, la sécurité, la vie privée et une infrastructure numérique résiliente à la maison.')
        ) AS spec(id, locale_code, source_id, slug, name, description)
        JOIN locales AS locale ON locale.code = spec.locale_code
        ON CONFLICT (locale_id, slug) DO NOTHING;

        WITH smart_home_groups AS (
          SELECT article_group_id FROM article_localizations
          WHERE locale_id = (SELECT id FROM locales WHERE code = 'tr-TR')
            AND slug ~ '(akilli-ev|robot-supurge|akilli-televizyon|ev-aginda|ev-tipi-nas|evde-yerel-yapay-zeka|elektrik-kesintisinde-ev-interneti|akilli-sayac|dijital-otomobil-anahtari)'
        )
        INSERT INTO article_categories (article_localization_id, category_id)
        SELECT article.id, category.id FROM article_localizations AS article
        JOIN smart_home_groups ON smart_home_groups.article_group_id = article.article_group_id
        JOIN locales AS locale ON locale.id = article.locale_id
        JOIN categories AS category ON category.locale_id = locale.id AND category.slug = CASE locale.code
          WHEN 'tr-TR' THEN 'akilli-ev-ve-baglantili-yasam' WHEN 'en-US' THEN 'smart-home-and-connected-living'
          WHEN 'de-DE' THEN 'smart-home-und-vernetztes-leben' WHEN 'fr-FR' THEN 'maison-intelligente-et-vie-connectee' END
        ON CONFLICT DO NOTHING;

        INSERT INTO audit_logs (id, actor_user_id, action, entity_type, entity_id, details_json, occurred_at)
        SELECT gen_random_uuid(), NULL, 'migration.smart_home_connected_living_taxonomy_added', 'Category', id,
          jsonb_build_object('slug', slug, 'localeId', locale_id), NOW() FROM categories
        WHERE id IN ('019c1f42-0000-7000-8000-000000000001','019c1f42-0000-7000-8000-000000000002','019c1f42-0000-7000-8000-000000000003','019c1f42-0000-7000-8000-000000000004');
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DELETE FROM categories WHERE id IN ('019c1f42-0000-7000-8000-000000000002','019c1f42-0000-7000-8000-000000000003','019c1f42-0000-7000-8000-000000000004');
        DELETE FROM categories WHERE id = '019c1f42-0000-7000-8000-000000000001';
        """);
}

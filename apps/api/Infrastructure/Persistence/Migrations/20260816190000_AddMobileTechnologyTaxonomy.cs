using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Peletnapechkai.Api.Infrastructure.Persistence;

#nullable disable

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations;

/// <summary>Adds the localized Mobile Technology topic and classifies matching published article groups.</summary>
[DbContext(typeof(PublishingDbContext))]
[Migration("20260816190000_AddMobileTechnologyTaxonomy")]
public partial class AddMobileTechnologyTaxonomy : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        INSERT INTO categories (id, locale_id, source_category_id, slug, name, description, created_at)
        SELECT spec.id, locale.id, spec.source_id, spec.slug, spec.name, spec.description, NOW()
        FROM (VALUES
          ('019c1f00-0000-7000-8000-000000000001'::uuid, 'tr-TR', NULL::uuid, 'mobil-teknoloji', 'Mobil Teknoloji', 'Telefonlar, giyilebilir cihazlar, mobil bağlantı ve kişisel cihaz güvenliği için rehberler, incelemeler ve güncel gelişmeler.'),
          ('019c1f00-0000-7000-8000-000000000002'::uuid, 'en-US', '019c1f00-0000-7000-8000-000000000001'::uuid, 'mobile-technology', 'Mobile Technology', 'Guides, reviews, and current developments covering phones, wearables, mobile connectivity, and personal device security.'),
          ('019c1f00-0000-7000-8000-000000000003'::uuid, 'de-DE', '019c1f00-0000-7000-8000-000000000001'::uuid, 'mobiltechnologie', 'Mobiltechnologie', 'Ratgeber, Tests und aktuelle Entwicklungen zu Smartphones, Wearables, mobiler Konnektivität und Gerätesicherheit.'),
          ('019c1f00-0000-7000-8000-000000000004'::uuid, 'fr-FR', '019c1f00-0000-7000-8000-000000000001'::uuid, 'technologie-mobile', 'Technologie mobile', 'Guides, essais et actualités sur les téléphones, les objets portables, la connectivité mobile et la sécurité des appareils personnels.')
        ) AS spec(id, locale_code, source_id, slug, name, description)
        JOIN locales AS locale ON locale.code = spec.locale_code
        ON CONFLICT (locale_id, slug) DO NOTHING;

        WITH mobile_groups AS (
          SELECT article_group_id FROM article_localizations
          WHERE locale_id = (SELECT id FROM locales WHERE code = 'tr-TR')
            AND (title ~* '(telefon|android|iphone|ios|mobil|eSIM|SIM kart|uydu iletişim|RCS|akıllı yüzük|giyilebilir|powerbank|Qi2|Bluetooth)')
        )
        INSERT INTO article_categories (article_localization_id, category_id)
        SELECT article.id, category.id
        FROM article_localizations AS article
        JOIN mobile_groups ON mobile_groups.article_group_id = article.article_group_id
        JOIN locales AS locale ON locale.id = article.locale_id
        JOIN categories AS category ON category.locale_id = locale.id
          AND category.slug = CASE locale.code
            WHEN 'tr-TR' THEN 'mobil-teknoloji' WHEN 'en-US' THEN 'mobile-technology'
            WHEN 'de-DE' THEN 'mobiltechnologie' WHEN 'fr-FR' THEN 'technologie-mobile' END
        ON CONFLICT DO NOTHING;

        INSERT INTO audit_logs (id, actor_user_id, action, entity_type, entity_id, details_json, occurred_at)
        SELECT gen_random_uuid(), NULL, 'migration.mobile_technology_taxonomy_added', 'Category', id,
          jsonb_build_object('slug', slug, 'localeId', locale_id), NOW()
        FROM categories WHERE id IN (
          '019c1f00-0000-7000-8000-000000000001','019c1f00-0000-7000-8000-000000000002',
          '019c1f00-0000-7000-8000-000000000003','019c1f00-0000-7000-8000-000000000004');
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DELETE FROM categories WHERE id IN (
          '019c1f00-0000-7000-8000-000000000002','019c1f00-0000-7000-8000-000000000003','019c1f00-0000-7000-8000-000000000004');
        DELETE FROM categories WHERE id = '019c1f00-0000-7000-8000-000000000001';
        """);
}

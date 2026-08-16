using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations;

/// <summary>Adds a localized software and applications topic and classifies the existing archive.</summary>
[DbContext(typeof(PublishingDbContext))]
[Migration("20260816203000_AddSoftwareApplicationsTaxonomy")]
public partial class AddSoftwareApplicationsTaxonomy : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        INSERT INTO categories (id, locale_id, source_category_id, slug, name, description, created_at)
        SELECT spec.id, locale.id, spec.source_id, spec.slug, spec.name, spec.description, NOW()
        FROM (VALUES
          ('019c1f10-0000-7000-8000-000000000001'::uuid, 'tr-TR', NULL::uuid, 'yazilim-ve-uygulamalar', 'Yazılım ve Uygulamalar', 'İşletim sistemleri, üretkenlik araçları, tarayıcılar, mesajlaşma uygulamaları ve gündelik yazılımlar için rehberler, incelemeler ve güvenli kullanım önerileri.'),
          ('019c1f10-0000-7000-8000-000000000002'::uuid, 'en-US', '019c1f10-0000-7000-8000-000000000001'::uuid, 'software-and-apps', 'Software and Apps', 'Guides, reviews, and safer-use advice for operating systems, productivity tools, browsers, messaging apps, and everyday software.'),
          ('019c1f10-0000-7000-8000-000000000003'::uuid, 'de-DE', '019c1f10-0000-7000-8000-000000000001'::uuid, 'software-und-apps', 'Software und Apps', 'Ratgeber, Tests und Sicherheitstipps zu Betriebssystemen, Produktivitätswerkzeugen, Browsern, Messengern und alltäglicher Software.'),
          ('019c1f10-0000-7000-8000-000000000004'::uuid, 'fr-FR', '019c1f10-0000-7000-8000-000000000001'::uuid, 'logiciels-et-applications', 'Logiciels et applications', 'Guides, essais et conseils de sécurité sur les systèmes, outils de productivité, navigateurs, messageries et logiciels du quotidien.')
        ) AS spec(id, locale_code, source_id, slug, name, description)
        JOIN locales AS locale ON locale.code = spec.locale_code
        ON CONFLICT (locale_id, slug) DO NOTHING;

        WITH software_groups AS (
          SELECT article_group_id FROM article_localizations
          WHERE locale_id = (SELECT id FROM locales WHERE code = 'tr-TR')
            AND (title ~* '(uygulama|yazılım|işletim sistemi|Windows|macOS|Linux|tarayıcı|browser|Office|Excel|WhatsApp|Telegram|Signal|VPN|parola yöneticisi|not alma|e-posta)')
        )
        INSERT INTO article_categories (article_localization_id, category_id)
        SELECT article.id, category.id FROM article_localizations AS article
        JOIN software_groups ON software_groups.article_group_id = article.article_group_id
        JOIN locales AS locale ON locale.id = article.locale_id
        JOIN categories AS category ON category.locale_id = locale.id AND category.slug = CASE locale.code
          WHEN 'tr-TR' THEN 'yazilim-ve-uygulamalar' WHEN 'en-US' THEN 'software-and-apps'
          WHEN 'de-DE' THEN 'software-und-apps' WHEN 'fr-FR' THEN 'logiciels-et-applications' END
        ON CONFLICT DO NOTHING;

        INSERT INTO audit_logs (id, actor_user_id, action, entity_type, entity_id, details_json, occurred_at)
        SELECT gen_random_uuid(), NULL, 'migration.software_applications_taxonomy_added', 'Category', id,
          jsonb_build_object('slug', slug, 'localeId', locale_id), NOW() FROM categories
        WHERE id IN ('019c1f10-0000-7000-8000-000000000001','019c1f10-0000-7000-8000-000000000002','019c1f10-0000-7000-8000-000000000003','019c1f10-0000-7000-8000-000000000004');
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DELETE FROM categories WHERE id IN ('019c1f10-0000-7000-8000-000000000002','019c1f10-0000-7000-8000-000000000003','019c1f10-0000-7000-8000-000000000004');
        DELETE FROM categories WHERE id = '019c1f10-0000-7000-8000-000000000001';
        """);
}

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations;

/// <summary>Adds a localized privacy and digital rights topic and classifies the proven archive cluster.</summary>
[DbContext(typeof(PublishingDbContext))]
[Migration("20260816220000_AddPrivacyDigitalRightsTaxonomy")]
public partial class AddPrivacyDigitalRightsTaxonomy : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        INSERT INTO categories (id, locale_id, source_category_id, slug, name, description, created_at)
        SELECT spec.id, locale.id, spec.source_id, spec.slug, spec.name, spec.description, NOW()
        FROM (VALUES
          ('019c1f34-0000-7000-8000-000000000001'::uuid, 'tr-TR', NULL::uuid, 'gizlilik-ve-dijital-haklar', 'Gizlilik ve Dijital Haklar', 'Kişisel verileri koruma, çevrimiçi takip, şifreleme, dijital kimlik ve teknoloji kullanırken sahip olduğunuz haklar için uygulamalı rehberler ve incelemeler.'),
          ('019c1f34-0000-7000-8000-000000000002'::uuid, 'en-US', '019c1f34-0000-7000-8000-000000000001'::uuid, 'privacy-and-digital-rights', 'Privacy and Digital Rights', 'Practical guides and reviews for protecting personal data, limiting online tracking, using encryption, managing digital identity, and understanding your rights.'),
          ('019c1f34-0000-7000-8000-000000000003'::uuid, 'de-DE', '019c1f34-0000-7000-8000-000000000001'::uuid, 'datenschutz-und-digitale-rechte', 'Datenschutz und digitale Rechte', 'Praxisnahe Ratgeber und Tests zu Datenschutz, Online-Tracking, Verschlüsselung, digitaler Identität und Rechten bei der Techniknutzung.'),
          ('019c1f34-0000-7000-8000-000000000004'::uuid, 'fr-FR', '019c1f34-0000-7000-8000-000000000001'::uuid, 'vie-privee-et-droits-numeriques', 'Vie privée et droits numériques', 'Guides pratiques et essais sur la protection des données, le pistage en ligne, le chiffrement, l’identité numérique et les droits liés aux technologies.')
        ) AS spec(id, locale_code, source_id, slug, name, description)
        JOIN locales AS locale ON locale.code = spec.locale_code
        ON CONFLICT (locale_id, slug) DO NOTHING;

        WITH privacy_groups AS (
          SELECT article_group_id FROM article_localizations
          WHERE locale_id = (SELECT id FROM locales WHERE code = 'tr-TR')
            AND slug ~ '(gizlilik|parola|sifre|vpn|uygulama-izinleri|takip-cihazi|dijital-kimlik|yas-dogrulama|verilerini-koruma|sifreli-dns|sifreli-yedek|c2pa)'
        )
        INSERT INTO article_categories (article_localization_id, category_id)
        SELECT article.id, category.id FROM article_localizations AS article
        JOIN privacy_groups ON privacy_groups.article_group_id = article.article_group_id
        JOIN locales AS locale ON locale.id = article.locale_id
        JOIN categories AS category ON category.locale_id = locale.id AND category.slug = CASE locale.code
          WHEN 'tr-TR' THEN 'gizlilik-ve-dijital-haklar' WHEN 'en-US' THEN 'privacy-and-digital-rights'
          WHEN 'de-DE' THEN 'datenschutz-und-digitale-rechte' WHEN 'fr-FR' THEN 'vie-privee-et-droits-numeriques' END
        ON CONFLICT DO NOTHING;

        INSERT INTO audit_logs (id, actor_user_id, action, entity_type, entity_id, details_json, occurred_at)
        SELECT gen_random_uuid(), NULL, 'migration.privacy_digital_rights_taxonomy_added', 'Category', id,
          jsonb_build_object('slug', slug, 'localeId', locale_id), NOW() FROM categories
        WHERE id IN ('019c1f34-0000-7000-8000-000000000001','019c1f34-0000-7000-8000-000000000002','019c1f34-0000-7000-8000-000000000003','019c1f34-0000-7000-8000-000000000004');
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DELETE FROM categories WHERE id IN ('019c1f34-0000-7000-8000-000000000002','019c1f34-0000-7000-8000-000000000003','019c1f34-0000-7000-8000-000000000004');
        DELETE FROM categories WHERE id = '019c1f34-0000-7000-8000-000000000001';
        """);
}

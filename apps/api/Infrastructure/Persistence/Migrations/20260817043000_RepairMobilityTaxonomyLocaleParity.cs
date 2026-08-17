using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations;

/// <summary>Repairs locale taxonomy rows when an environment does not yet contain every localized parent.</summary>
[DbContext(typeof(PublishingDbContext))]
[Migration("20260817043000_RepairMobilityTaxonomyLocaleParity")]
public partial class RepairMobilityTaxonomyLocaleParity : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        INSERT INTO categories (id, locale_id, source_category_id, parent_category_id, slug, name, description, created_at)
        SELECT spec.id, locale.id, spec.source_id, parent.id, spec.slug, spec.name, spec.description, NOW()
        FROM (VALUES
          ('01a00bc0-0000-7000-8000-000000000001'::uuid, 'tr-TR', NULL::uuid, 'donanim', 'otomobil-teknolojileri-ve-mobilite', 'Otomobil Teknolojileri ve Mobilite', 'Bağlantılı otomobiller, araç verisi, sürüş teknolojileri, elektrikli araç şarjı ve güvenli dijital mobilite için rehberler ve analizler.'),
          ('01a00bc0-0000-7000-8000-000000000002'::uuid, 'en-US', '01a00bc0-0000-7000-8000-000000000001'::uuid, 'hardware', 'automotive-technology-and-mobility', 'Automotive Technology and Mobility', 'Guides and analysis for connected cars, vehicle data, driving technology, EV charging, and secure digital mobility.'),
          ('01a00bc0-0000-7000-8000-000000000003'::uuid, 'de-DE', '01a00bc0-0000-7000-8000-000000000001'::uuid, 'hardware', 'automobiltechnik-und-mobilitaet', 'Automobiltechnik und Mobilität', 'Ratgeber und Analysen zu vernetzten Autos, Fahrzeugdaten, Fahrtechnologien, E-Auto-Laden und sicherer digitaler Mobilität.'),
          ('01a00bc0-0000-7000-8000-000000000004'::uuid, 'fr-FR', '01a00bc0-0000-7000-8000-000000000001'::uuid, 'materiel', 'technologies-automobiles-et-mobilite', 'Technologies automobiles et mobilité', 'Guides et analyses sur les voitures connectées, les données du véhicule, la recharge électrique et la mobilité numérique sûre.')
        ) AS spec(id, locale_code, source_id, parent_slug, slug, name, description)
        JOIN locales AS locale ON locale.code = spec.locale_code
        LEFT JOIN categories AS parent ON parent.locale_id = locale.id AND parent.slug = spec.parent_slug
        ON CONFLICT (locale_id, slug) DO NOTHING;

        UPDATE categories AS child SET parent_category_id = parent.id
        FROM categories AS parent
        WHERE child.locale_id = parent.locale_id AND child.parent_category_id IS NULL AND (
          (parent.slug IN ('dijital-yasam','digital-life','digitales-leben','vie-numerique') AND child.slug IN ('verimlilik','productivity','produktivitaet','productivite','gizlilik-ve-dijital-haklar','privacy-and-digital-rights','datenschutz-und-digitale-rechte','vie-privee-et-droits-numeriques')) OR
          (parent.slug IN ('yazilim-ve-uygulamalar','software-and-apps','software-und-apps','logiciels-et-applications') AND child.slug IN ('yapay-zeka','artificial-intelligence','kuenstliche-intelligenz','intelligence-artificielle','siber-guvenlik','cybersecurity','cybersicherheit','cybersecurite')) OR
          (parent.slug IN ('donanim','hardware','materiel') AND child.slug IN ('mobil-teknoloji','mobile-technology','mobiltechnologie','technologie-mobile','akilli-ev-ve-baglantili-yasam','smart-home-and-connected-living','smart-home-und-vernetztes-leben','maison-intelligente-et-vie-connectee','otomobil-teknolojileri-ve-mobilite','automotive-technology-and-mobility','automobiltechnik-und-mobilitaet','technologies-automobiles-et-mobilite'))
        );

        INSERT INTO audit_logs (id, actor_user_id, action, entity_type, entity_id, details_json, occurred_at)
        SELECT gen_random_uuid(), NULL, 'migration.mobility_taxonomy_locale_parity_repaired', 'Category', id,
          jsonb_build_object('slug', slug, 'parentCategoryId', parent_category_id), NOW()
        FROM categories WHERE id IN ('01a00bc0-0000-7000-8000-000000000001','01a00bc0-0000-7000-8000-000000000002','01a00bc0-0000-7000-8000-000000000003','01a00bc0-0000-7000-8000-000000000004')
          AND NOT EXISTS (SELECT 1 FROM audit_logs WHERE action = 'migration.mobility_taxonomy_locale_parity_repaired' AND entity_id = categories.id);
        """);

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // The schema migration owns rollback of these taxonomy rows. Repair is intentionally data-preserving.
    }
}

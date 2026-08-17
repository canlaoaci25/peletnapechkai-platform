using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryHierarchyAndMobilityTaxonomy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "parent_category_id",
                table: "categories",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_categories_parent_name",
                table: "categories",
                columns: new[] { "parent_category_id", "name" });

            migrationBuilder.AddForeignKey(
                name: "FK_categories_categories_parent_category_id",
                table: "categories",
                column: "parent_category_id",
                principalTable: "categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql("""
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

                WITH mobility_groups AS (
                  SELECT article_group_id FROM article_localizations
                  WHERE locale_id = (SELECT id FROM locales WHERE code = 'tr-TR')
                    AND slug ~ '(obd-ii|arac-kazasindan-sonra-dijital|android-automotive|ecall|dijital-otomobil-anahtari|kiralik-otomobilde|akilli-ev-tipi-elektrikli-arac)'
                )
                INSERT INTO article_categories (article_localization_id, category_id)
                SELECT article.id, category.id FROM article_localizations AS article
                JOIN mobility_groups ON mobility_groups.article_group_id = article.article_group_id
                JOIN locales AS locale ON locale.id = article.locale_id
                JOIN categories AS category ON category.locale_id = locale.id AND category.slug = CASE locale.code
                  WHEN 'tr-TR' THEN 'otomobil-teknolojileri-ve-mobilite'
                  WHEN 'en-US' THEN 'automotive-technology-and-mobility'
                  WHEN 'de-DE' THEN 'automobiltechnik-und-mobilitaet'
                  WHEN 'fr-FR' THEN 'technologies-automobiles-et-mobilite' END
                ON CONFLICT DO NOTHING;

                UPDATE categories AS child SET parent_category_id = parent.id
                FROM categories AS parent
                WHERE child.locale_id = parent.locale_id AND (
                  (parent.slug IN ('dijital-yasam','digital-life','digitales-leben','vie-numerique') AND child.slug IN ('verimlilik','productivity','produktivitaet','productivite','gizlilik-ve-dijital-haklar','privacy-and-digital-rights','datenschutz-und-digitale-rechte','vie-privee-et-droits-numeriques')) OR
                  (parent.slug IN ('yazilim-ve-uygulamalar','software-and-apps','software-und-apps','logiciels-et-applications') AND child.slug IN ('yapay-zeka','artificial-intelligence','kuenstliche-intelligenz','intelligence-artificielle','siber-guvenlik','cybersecurity','cybersicherheit','cybersecurite')) OR
                  (parent.slug IN ('donanim','hardware','materiel') AND child.slug IN ('mobil-teknoloji','mobile-technology','mobiltechnologie','technologie-mobile','akilli-ev-ve-baglantili-yasam','smart-home-and-connected-living','smart-home-und-vernetztes-leben','maison-intelligente-et-vie-connectee','otomobil-teknolojileri-ve-mobilite','automotive-technology-and-mobility','automobiltechnik-und-mobilitaet','technologies-automobiles-et-mobilite'))
                );

                INSERT INTO audit_logs (id, actor_user_id, action, entity_type, entity_id, details_json, occurred_at)
                SELECT gen_random_uuid(), NULL, 'migration.category_hierarchy_mobility_added', 'Category', id,
                  jsonb_build_object('slug', slug, 'parentCategoryId', parent_category_id), NOW()
                FROM categories WHERE id IN ('01a00bc0-0000-7000-8000-000000000001','01a00bc0-0000-7000-8000-000000000002','01a00bc0-0000-7000-8000-000000000003','01a00bc0-0000-7000-8000-000000000004');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE categories SET parent_category_id = NULL WHERE parent_category_id IS NOT NULL;
                DELETE FROM categories WHERE id IN ('01a00bc0-0000-7000-8000-000000000004','01a00bc0-0000-7000-8000-000000000003','01a00bc0-0000-7000-8000-000000000002','01a00bc0-0000-7000-8000-000000000001');
                """);
            migrationBuilder.DropForeignKey(
                name: "FK_categories_categories_parent_category_id",
                table: "categories");

            migrationBuilder.DropIndex(
                name: "ix_categories_parent_name",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "parent_category_id",
                table: "categories");
        }
    }
}

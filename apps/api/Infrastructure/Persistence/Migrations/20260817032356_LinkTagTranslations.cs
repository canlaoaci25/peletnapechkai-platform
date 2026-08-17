using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LinkTagTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "source_tag_id",
                table: "tags",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ux_tags_source_locale",
                table: "tags",
                columns: new[] { "source_tag_id", "locale_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tags_tags_source_tag_id",
                table: "tags",
                column: "source_tag_id",
                principalTable: "tags",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql("""
                WITH translations(source_slug, locale_code, slug, name) AS (VALUES
                  ('alisveris','en-US','shopping','Shopping'),('alisveris','de-DE','einkaufen','Einkaufen'),('alisveris','fr-FR','achats','Achats'),
                  ('gizlilik','en-US','privacy','Privacy'),('gizlilik','de-DE','datenschutz','Datenschutz'),('gizlilik','fr-FR','confidentialite','ConfidentialitÃ©'),
                  ('guvenlik','en-US','security','Security'),('guvenlik','de-DE','sicherheit','Sicherheit'),('guvenlik','fr-FR','securite','SÃ©curitÃ©'),
                  ('mobil','en-US','mobile','Mobile'),('mobil','de-DE','mobil','Mobil'),('mobil','fr-FR','mobile','Mobile'),
                  ('rehber','en-US','guide','Guide'),('rehber','de-DE','ratgeber','Ratgeber'),('rehber','fr-FR','guide','Guide'),
                  ('surdurulebilirlik','en-US','sustainability','Sustainability'),('surdurulebilirlik','de-DE','nachhaltigkeit','Nachhaltigkeit'),('surdurulebilirlik','fr-FR','durabilite','DurabilitÃ©'),
                  ('windows','en-US','windows','Windows'),('windows','de-DE','windows','Windows'),('windows','fr-FR','windows','Windows')
                ), inserted AS (
                  INSERT INTO tags (id, locale_id, source_tag_id, slug, name, created_at)
                  SELECT gen_random_uuid(), target_locale.id, source.id, translations.slug, translations.name, now()
                  FROM translations
                  JOIN locales source_locale ON source_locale.code = 'tr-TR'
                  JOIN tags source ON source.locale_id = source_locale.id AND source.slug = translations.source_slug
                  JOIN locales target_locale ON target_locale.code = translations.locale_code
                  WHERE NOT EXISTS (SELECT 1 FROM tags existing WHERE existing.source_tag_id = source.id AND existing.locale_id = target_locale.id)
                  RETURNING id
                )
                INSERT INTO audit_logs (id, actor_user_id, action, entity_type, entity_id, details_json, occurred_at)
                SELECT gen_random_uuid(), NULL, 'migration.tag_translation_added', 'Tag', id, '{"cycle":46}'::jsonb, now() FROM inserted;

                INSERT INTO article_tags (article_localization_id, tag_id)
                SELECT target_article.id, target_tag.id
                FROM tags target_tag
                JOIN tags source_tag ON source_tag.id = target_tag.source_tag_id
                JOIN article_tags source_link ON source_link.tag_id = source_tag.id
                JOIN article_localizations source_article ON source_article.id = source_link.article_localization_id
                JOIN article_localizations target_article ON target_article.article_group_id = source_article.article_group_id AND target_article.locale_id = target_tag.locale_id
                ON CONFLICT DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM tags WHERE id IN (
                  SELECT entity_id FROM audit_logs WHERE action = 'migration.tag_translation_added' AND entity_type = 'Tag'
                );
                DELETE FROM audit_logs WHERE action = 'migration.tag_translation_added' AND entity_type = 'Tag';
                """);
            migrationBuilder.DropForeignKey(
                name: "FK_tags_tags_source_tag_id",
                table: "tags");

            migrationBuilder.DropIndex(
                name: "ux_tags_source_locale",
                table: "tags");

            migrationBuilder.DropColumn(
                name: "source_tag_id",
                table: "tags");
        }
    }
}

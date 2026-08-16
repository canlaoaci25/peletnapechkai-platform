using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LinkCategoryTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "source_category_id",
                table: "categories",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE categories AS target
                SET source_category_id = log.entity_id
                FROM audit_logs AS log
                WHERE log.action = 'automation.category_localized'
                  AND log.details_json IS NOT NULL
                  AND target.id = NULLIF(log.details_json::jsonb ->> 'translatedCategoryId', '')::uuid
                  AND target.source_category_id IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "ux_categories_source_locale",
                table: "categories",
                columns: new[] { "source_category_id", "locale_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_categories_categories_source_category_id",
                table: "categories",
                column: "source_category_id",
                principalTable: "categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_categories_categories_source_category_id",
                table: "categories");

            migrationBuilder.DropIndex(
                name: "ux_categories_source_locale",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "source_category_id",
                table: "categories");
        }
    }
}

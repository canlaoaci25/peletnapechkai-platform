using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTranslationSourceSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "source_body_hash",
                table: "article_localizations",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source_seo_hash",
                table: "article_localizations",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "source_snapshot_updated_at",
                table: "article_localizations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source_summary_hash",
                table: "article_localizations",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source_title_hash",
                table: "article_localizations",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "source_body_hash",
                table: "article_localizations");

            migrationBuilder.DropColumn(
                name: "source_seo_hash",
                table: "article_localizations");

            migrationBuilder.DropColumn(
                name: "source_snapshot_updated_at",
                table: "article_localizations");

            migrationBuilder.DropColumn(
                name: "source_summary_hash",
                table: "article_localizations");

            migrationBuilder.DropColumn(
                name: "source_title_hash",
                table: "article_localizations");
        }
    }
}

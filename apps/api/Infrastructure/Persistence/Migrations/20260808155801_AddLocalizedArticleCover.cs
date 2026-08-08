using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLocalizedArticleCover : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cover_alt_text",
                table: "article_localizations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cover_caption",
                table: "article_localizations",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cover_credit",
                table: "article_localizations",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "cover_media_asset_id",
                table: "article_localizations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_article_localizations_cover_media_asset_id",
                table: "article_localizations",
                column: "cover_media_asset_id");

            migrationBuilder.AddForeignKey(
                name: "FK_article_localizations_media_assets_cover_media_asset_id",
                table: "article_localizations",
                column: "cover_media_asset_id",
                principalTable: "media_assets",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_article_localizations_media_assets_cover_media_asset_id",
                table: "article_localizations");

            migrationBuilder.DropIndex(
                name: "IX_article_localizations_cover_media_asset_id",
                table: "article_localizations");

            migrationBuilder.DropColumn(
                name: "cover_alt_text",
                table: "article_localizations");

            migrationBuilder.DropColumn(
                name: "cover_caption",
                table: "article_localizations");

            migrationBuilder.DropColumn(
                name: "cover_credit",
                table: "article_localizations");

            migrationBuilder.DropColumn(
                name: "cover_media_asset_id",
                table: "article_localizations");
        }
    }
}

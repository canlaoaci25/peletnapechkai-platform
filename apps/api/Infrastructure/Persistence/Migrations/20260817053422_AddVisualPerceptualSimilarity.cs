using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVisualPerceptualSimilarity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "closest_media_asset_id",
                table: "visual_review_tasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "closest_similarity_percent",
                table: "visual_review_tasks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "perceptual_hash",
                table: "media_assets",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_visual_review_tasks_closest_media_asset_id",
                table: "visual_review_tasks",
                column: "closest_media_asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_media_assets_perceptual_hash",
                table: "media_assets",
                column: "perceptual_hash");

            migrationBuilder.AddForeignKey(
                name: "FK_visual_review_tasks_media_assets_closest_media_asset_id",
                table: "visual_review_tasks",
                column: "closest_media_asset_id",
                principalTable: "media_assets",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_visual_review_tasks_media_assets_closest_media_asset_id",
                table: "visual_review_tasks");

            migrationBuilder.DropIndex(
                name: "IX_visual_review_tasks_closest_media_asset_id",
                table: "visual_review_tasks");

            migrationBuilder.DropIndex(
                name: "ix_media_assets_perceptual_hash",
                table: "media_assets");

            migrationBuilder.DropColumn(
                name: "closest_media_asset_id",
                table: "visual_review_tasks");

            migrationBuilder.DropColumn(
                name: "closest_similarity_percent",
                table: "visual_review_tasks");

            migrationBuilder.DropColumn(
                name: "perceptual_hash",
                table: "media_assets");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVisualCandidatePromotion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "attribution",
                table: "visual_review_tasks",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "candidate_alt_text",
                table: "visual_review_tasks",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "candidate_media_asset_id",
                table: "visual_review_tasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "crop_score",
                table: "visual_review_tasks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "license_name",
                table: "visual_review_tasks",
                type: "character varying(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "originality_score",
                table: "visual_review_tasks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "promoted_at",
                table: "visual_review_tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "provider",
                table: "visual_review_tasks",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "text_safety_score",
                table: "visual_review_tasks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "topic_score",
                table: "visual_review_tasks",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_visual_review_tasks_candidate_media_asset_id",
                table: "visual_review_tasks",
                column: "candidate_media_asset_id");

            migrationBuilder.AddForeignKey(
                name: "FK_visual_review_tasks_media_assets_candidate_media_asset_id",
                table: "visual_review_tasks",
                column: "candidate_media_asset_id",
                principalTable: "media_assets",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_visual_review_tasks_media_assets_candidate_media_asset_id",
                table: "visual_review_tasks");

            migrationBuilder.DropIndex(
                name: "IX_visual_review_tasks_candidate_media_asset_id",
                table: "visual_review_tasks");

            migrationBuilder.DropColumn(
                name: "attribution",
                table: "visual_review_tasks");

            migrationBuilder.DropColumn(
                name: "candidate_alt_text",
                table: "visual_review_tasks");

            migrationBuilder.DropColumn(
                name: "candidate_media_asset_id",
                table: "visual_review_tasks");

            migrationBuilder.DropColumn(
                name: "crop_score",
                table: "visual_review_tasks");

            migrationBuilder.DropColumn(
                name: "license_name",
                table: "visual_review_tasks");

            migrationBuilder.DropColumn(
                name: "originality_score",
                table: "visual_review_tasks");

            migrationBuilder.DropColumn(
                name: "promoted_at",
                table: "visual_review_tasks");

            migrationBuilder.DropColumn(
                name: "provider",
                table: "visual_review_tasks");

            migrationBuilder.DropColumn(
                name: "text_safety_score",
                table: "visual_review_tasks");

            migrationBuilder.DropColumn(
                name: "topic_score",
                table: "visual_review_tasks");
        }
    }
}

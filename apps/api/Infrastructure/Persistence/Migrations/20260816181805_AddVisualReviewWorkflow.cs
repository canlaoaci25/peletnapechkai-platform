using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVisualReviewWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "visual_review_tasks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    article_localization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_media_asset_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quality_score = table.Column<int>(type: "integer", nullable: false),
                    risks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    section_context = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    visual_purpose = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    proposed_prompt = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    negative_prompt = table.Column<string>(type: "character varying(1200)", maxLength: 1200, nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    attempt_count = table.Column<int>(type: "integer", nullable: false),
                    reviewer_note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    reviewed_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reviewed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visual_review_tasks", x => x.id);
                    table.ForeignKey(
                        name: "FK_visual_review_tasks_article_localizations_article_localizat~",
                        column: x => x.article_localization_id,
                        principalTable: "article_localizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_visual_review_tasks_media_assets_current_media_asset_id",
                        column: x => x.current_media_asset_id,
                        principalTable: "media_assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_visual_review_tasks_article_localization_id",
                table: "visual_review_tasks",
                column: "article_localization_id");

            migrationBuilder.CreateIndex(
                name: "IX_visual_review_tasks_current_media_asset_id",
                table: "visual_review_tasks",
                column: "current_media_asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_visual_review_tasks_queue",
                table: "visual_review_tasks",
                columns: new[] { "status", "quality_score", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ux_visual_review_tasks_idempotency",
                table: "visual_review_tasks",
                column: "idempotency_key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "visual_review_tasks");
        }
    }
}

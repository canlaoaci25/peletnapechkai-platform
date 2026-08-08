using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEditorialOperations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "article_quality_checklists",
                columns: table => new
                {
                    article_localization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title_and_summary = table.Column<bool>(type: "boolean", nullable: false),
                    sources_verified = table.Column<bool>(type: "boolean", nullable: false),
                    author_and_taxonomy = table.Column<bool>(type: "boolean", nullable: false),
                    seo_metadata = table.Column<bool>(type: "boolean", nullable: false),
                    cover_accessibility = table.Column<bool>(type: "boolean", nullable: false),
                    commercial_disclosure = table.Column<bool>(type: "boolean", nullable: false),
                    translation_reviewed = table.Column<bool>(type: "boolean", nullable: false),
                    legal_editorial_review = table.Column<bool>(type: "boolean", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_article_quality_checklists", x => x.article_localization_id);
                    table.ForeignKey(
                        name: "FK_article_quality_checklists_article_localizations_article_lo~",
                        column: x => x.article_localization_id,
                        principalTable: "article_localizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "editorial_comments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    article_localization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    author_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    body = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    parent_comment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    article_revision_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_resolved = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_editorial_comments", x => x.id);
                    table.ForeignKey(
                        name: "FK_editorial_comments_article_localizations_article_localizati~",
                        column: x => x.article_localization_id,
                        principalTable: "article_localizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "editorial_tasks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    article_localization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assignee_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    priority = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    due_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_editorial_tasks", x => x.id);
                    table.ForeignKey(
                        name: "FK_editorial_tasks_article_localizations_article_localization_~",
                        column: x => x.article_localization_id,
                        principalTable: "article_localizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_editorial_comments_article_created",
                table: "editorial_comments",
                columns: new[] { "article_localization_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_editorial_tasks_article_localization_id",
                table: "editorial_tasks",
                column: "article_localization_id");

            migrationBuilder.CreateIndex(
                name: "ix_editorial_tasks_assignee_status_due",
                table: "editorial_tasks",
                columns: new[] { "assignee_user_id", "status", "due_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "article_quality_checklists");

            migrationBuilder.DropTable(
                name: "editorial_comments");

            migrationBuilder.DropTable(
                name: "editorial_tasks");
        }
    }
}

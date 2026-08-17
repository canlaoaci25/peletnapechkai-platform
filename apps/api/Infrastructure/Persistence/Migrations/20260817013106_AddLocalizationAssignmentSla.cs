using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLocalizationAssignmentSla : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "localization_assignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    article_group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_locale_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assignee_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    due_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_localization_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_localization_assignments_article_groups_article_group_id",
                        column: x => x.article_group_id,
                        principalTable: "article_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_localization_assignments_locales_target_locale_id",
                        column: x => x.target_locale_id,
                        principalTable: "locales",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_localization_assignments_target_locale_id",
                table: "localization_assignments",
                column: "target_locale_id");

            migrationBuilder.CreateIndex(
                name: "ix_localization_assignments_owner_sla",
                table: "localization_assignments",
                columns: new[] { "assignee_user_id", "status", "due_at" });

            migrationBuilder.CreateIndex(
                name: "ux_localization_assignments_group_locale",
                table: "localization_assignments",
                columns: new[] { "article_group_id", "target_locale_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "localization_assignments");
        }
    }
}

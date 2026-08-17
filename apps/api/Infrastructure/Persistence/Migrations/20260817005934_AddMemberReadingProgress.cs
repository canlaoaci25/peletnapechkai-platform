using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberReadingProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "article_reading_progress",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    article_localization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    percent = table.Column<int>(type: "integer", nullable: false),
                    anchor = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    last_read_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_article_reading_progress", x => x.id);
                    table.ForeignKey(
                        name: "FK_article_reading_progress_article_localizations_article_loca~",
                        column: x => x.article_localization_id,
                        principalTable: "article_localizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_article_reading_progress_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_article_reading_progress_article_localization_id",
                table: "article_reading_progress",
                column: "article_localization_id");

            migrationBuilder.CreateIndex(
                name: "ix_reading_progress_user_last_read",
                table: "article_reading_progress",
                columns: new[] { "user_id", "last_read_at" });

            migrationBuilder.CreateIndex(
                name: "ux_reading_progress_user_article",
                table: "article_reading_progress",
                columns: new[] { "user_id", "article_localization_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "article_reading_progress");
        }
    }
}

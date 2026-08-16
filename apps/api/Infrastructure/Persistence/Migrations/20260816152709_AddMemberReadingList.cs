using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberReadingList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "saved_articles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    article_localization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    saved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_saved_articles", x => x.id);
                    table.ForeignKey(
                        name: "FK_saved_articles_article_localizations_article_localization_id",
                        column: x => x.article_localization_id,
                        principalTable: "article_localizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_saved_articles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_saved_articles_article_localization_id",
                table: "saved_articles",
                column: "article_localization_id");

            migrationBuilder.CreateIndex(
                name: "ix_saved_articles_user_saved_at",
                table: "saved_articles",
                columns: new[] { "user_id", "saved_at" });

            migrationBuilder.CreateIndex(
                name: "ux_saved_articles_user_article",
                table: "saved_articles",
                columns: new[] { "user_id", "article_localization_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "saved_articles");
        }
    }
}

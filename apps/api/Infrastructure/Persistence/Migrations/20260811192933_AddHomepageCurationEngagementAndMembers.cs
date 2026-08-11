using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHomepageCurationEngagementAndMembers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "article_engagements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    article_localization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    view_count = table.Column<long>(type: "bigint", nullable: false),
                    engaged_seconds = table.Column<long>(type: "bigint", nullable: false),
                    last_viewed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_article_engagements", x => x.id);
                    table.ForeignKey(
                        name: "FK_article_engagements_article_localizations_article_localizat~",
                        column: x => x.article_localization_id,
                        principalTable: "article_localizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "homepage_placements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    locale_id = table.Column<Guid>(type: "uuid", nullable: false),
                    article_localization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    section = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_homepage_placements", x => x.id);
                    table.ForeignKey(
                        name: "FK_homepage_placements_article_localizations_article_localizat~",
                        column: x => x.article_localization_id,
                        principalTable: "article_localizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_homepage_placements_locales_locale_id",
                        column: x => x.locale_id,
                        principalTable: "locales",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "concurrency_stamp", "name", "normalized_name" },
                values: new object[] { new Guid("0198f100-0000-7000-a000-000000000007"), "role-member-v1", "Member", "MEMBER" });

            migrationBuilder.CreateIndex(
                name: "ux_article_engagements_article",
                table: "article_engagements",
                column: "article_localization_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_homepage_placements_article_localization_id",
                table: "homepage_placements",
                column: "article_localization_id");

            migrationBuilder.CreateIndex(
                name: "ux_homepage_placements_article",
                table: "homepage_placements",
                columns: new[] { "locale_id", "article_localization_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_homepage_placements_slot",
                table: "homepage_placements",
                columns: new[] { "locale_id", "section", "position" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "article_engagements");

            migrationBuilder.DropTable(
                name: "homepage_placements");

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("0198f100-0000-7000-a000-000000000007"));
        }
    }
}

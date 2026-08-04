using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialPublishingSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "article_groups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_article_groups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "regions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "locales",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    language_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    region_id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    native_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_locales", x => x.id);
                    table.ForeignKey(
                        name: "FK_locales_regions_region_id",
                        column: x => x.region_id,
                        principalTable: "regions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "article_localizations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    article_group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    locale_id = table.Column<Guid>(type: "uuid", nullable: false),
                    slug = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    title = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    summary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    seo_title = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                    seo_description = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    scheduled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_article_localizations", x => x.id);
                    table.CheckConstraint("ck_article_localizations_slug_not_empty", "length(trim(slug)) > 0");
                    table.CheckConstraint("ck_article_localizations_title_not_empty", "length(trim(title)) > 0");
                    table.ForeignKey(
                        name: "FK_article_localizations_article_groups_article_group_id",
                        column: x => x.article_group_id,
                        principalTable: "article_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_article_localizations_locales_locale_id",
                        column: x => x.locale_id,
                        principalTable: "locales",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "regions",
                columns: new[] { "id", "code", "currency_code", "is_enabled", "name" },
                values: new object[,]
                {
                    { new Guid("0198f100-0000-7000-8000-000000000001"), "TR", "TRY", true, "Türkiye" },
                    { new Guid("0198f100-0000-7000-8000-000000000002"), "US", "USD", true, "United States" },
                    { new Guid("0198f100-0000-7000-8000-000000000003"), "DE", "EUR", true, "Germany" }
                });

            migrationBuilder.InsertData(
                table: "locales",
                columns: new[] { "id", "code", "display_name", "is_default", "is_enabled", "language_code", "native_name", "region_id" },
                values: new object[,]
                {
                    { new Guid("0198f100-0000-7000-9000-000000000001"), "tr-TR", "Turkish (Türkiye)", true, true, "tr", "Türkçe (Türkiye)", new Guid("0198f100-0000-7000-8000-000000000001") },
                    { new Guid("0198f100-0000-7000-9000-000000000002"), "en-US", "English (United States)", false, true, "en", "English (United States)", new Guid("0198f100-0000-7000-8000-000000000002") },
                    { new Guid("0198f100-0000-7000-9000-000000000003"), "de-DE", "German (Germany)", false, true, "de", "Deutsch (Deutschland)", new Guid("0198f100-0000-7000-8000-000000000003") }
                });

            migrationBuilder.CreateIndex(
                name: "ix_article_localizations_publication",
                table: "article_localizations",
                columns: new[] { "locale_id", "status", "published_at" });

            migrationBuilder.CreateIndex(
                name: "ux_article_localizations_group_locale",
                table: "article_localizations",
                columns: new[] { "article_group_id", "locale_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_article_localizations_locale_slug",
                table: "article_localizations",
                columns: new[] { "locale_id", "slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_locales_region_id",
                table: "locales",
                column: "region_id");

            migrationBuilder.CreateIndex(
                name: "ux_locales_code",
                table: "locales",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_locales_single_default",
                table: "locales",
                column: "is_default",
                unique: true,
                filter: "is_default");

            migrationBuilder.CreateIndex(
                name: "ux_regions_code",
                table: "regions",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "article_localizations");

            migrationBuilder.DropTable(
                name: "article_groups");

            migrationBuilder.DropTable(
                name: "locales");

            migrationBuilder.DropTable(
                name: "regions");
        }
    }
}

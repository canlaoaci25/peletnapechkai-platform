using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLocaleCountryManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "locale_countries",
                columns: table => new
                {
                    locale_id = table.Column<Guid>(type: "uuid", nullable: false),
                    country_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_locale_countries", x => new { x.locale_id, x.country_id });
                    table.ForeignKey(
                        name: "FK_locale_countries_locales_locale_id",
                        column: x => x.locale_id,
                        principalTable: "locales",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_locale_countries_regions_country_id",
                        column: x => x.country_id,
                        principalTable: "regions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_locale_countries_country_enabled",
                table: "locale_countries",
                columns: new[] { "country_id", "is_enabled" });

            migrationBuilder.InsertData(
                table: "locale_countries",
                columns: new[] { "locale_id", "country_id", "is_required", "is_enabled" },
                values: new object[,]
                {
                    { Guid.Parse("0198F100-0000-7000-9000-000000000001"), Guid.Parse("0198F100-0000-7000-8000-000000000001"), true, true },
                    { Guid.Parse("0198F100-0000-7000-9000-000000000002"), Guid.Parse("0198F100-0000-7000-8000-000000000002"), true, true },
                    { Guid.Parse("0198F100-0000-7000-9000-000000000003"), Guid.Parse("0198F100-0000-7000-8000-000000000003"), true, true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "locale_countries");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddArticleCorrections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "article_corrections",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    article_localization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    summary = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    details = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    approved_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    corrected_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_article_corrections", x => x.id);
                    table.CheckConstraint("ck_article_corrections_details", "length(trim(details)) > 0");
                    table.CheckConstraint("ck_article_corrections_summary", "length(trim(summary)) > 0");
                    table.ForeignKey(
                        name: "FK_article_corrections_article_localizations_article_localizat~",
                        column: x => x.article_localization_id,
                        principalTable: "article_localizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_article_corrections_article_date",
                table: "article_corrections",
                columns: new[] { "article_localization_id", "corrected_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "article_corrections");
        }
    }
}

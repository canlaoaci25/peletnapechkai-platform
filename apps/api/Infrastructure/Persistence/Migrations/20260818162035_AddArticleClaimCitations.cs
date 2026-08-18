using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddArticleClaimCitations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "article_claim_citations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    article_localization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    locator = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    approved_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    approved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_article_claim_citations", x => x.id);
                    table.CheckConstraint("ck_article_claim_citations_claim_not_empty", "length(trim(claim)) > 0");
                    table.ForeignKey(
                        name: "FK_article_claim_citations_article_localizations_article_local~",
                        column: x => x.article_localization_id,
                        principalTable: "article_localizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_article_claim_citations_sources_source_id",
                        column: x => x.source_id,
                        principalTable: "sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_article_claim_citations_source_id",
                table: "article_claim_citations",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "ix_article_claim_citations_article_time",
                table: "article_claim_citations",
                columns: new[] { "article_localization_id", "approved_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "article_claim_citations");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSourceEditorialReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "kind",
                table: "sources",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Unclassified");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "last_reviewed_at",
                table: "sources",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "kind",
                table: "sources");

            migrationBuilder.DropColumn(
                name: "last_reviewed_at",
                table: "sources");
        }
    }
}

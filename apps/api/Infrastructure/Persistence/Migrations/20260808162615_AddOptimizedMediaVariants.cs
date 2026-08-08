using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOptimizedMediaVariants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "optimized_byte_length",
                table: "media_assets",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "optimized_storage_key",
                table: "media_assets",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "optimized_byte_length",
                table: "media_assets");

            migrationBuilder.DropColumn(
                name: "optimized_storage_key",
                table: "media_assets");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaFocalPoint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "focal_x",
                table: "media_assets",
                type: "numeric(5,4)",
                precision: 5,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "focal_y",
                table: "media_assets",
                type: "numeric(5,4)",
                precision: 5,
                scale: 4,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_media_assets_focal_point",
                table: "media_assets",
                sql: "(focal_x IS NULL AND focal_y IS NULL) OR (focal_x BETWEEN 0 AND 1 AND focal_y BETWEEN 0 AND 1)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_media_assets_focal_point",
                table: "media_assets");

            migrationBuilder.DropColumn(
                name: "focal_x",
                table: "media_assets");

            migrationBuilder.DropColumn(
                name: "focal_y",
                table: "media_assets");
        }
    }
}

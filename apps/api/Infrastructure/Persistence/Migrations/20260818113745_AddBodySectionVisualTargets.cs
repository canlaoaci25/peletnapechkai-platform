using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBodySectionVisualTargets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "section_heading",
                table: "visual_review_tasks",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "target",
                table: "visual_review_tasks",
                type: "character varying(24)",
                maxLength: 24,
                nullable: false,
                defaultValue: "Cover");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "section_heading",
                table: "visual_review_tasks");

            migrationBuilder.DropColumn(
                name: "target",
                table: "visual_review_tasks");
        }
    }
}

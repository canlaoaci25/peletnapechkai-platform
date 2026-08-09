using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAutomationJobReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "report_text",
                table: "automation_jobs",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "report_text",
                table: "automation_jobs");
        }
    }
}

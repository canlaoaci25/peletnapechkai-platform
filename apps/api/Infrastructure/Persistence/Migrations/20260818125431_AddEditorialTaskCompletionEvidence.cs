using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEditorialTaskCompletionEvidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "completed_at",
                table: "editorial_tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_editorial_tasks_completed_at",
                table: "editorial_tasks",
                column: "completed_at",
                filter: "status = 'Completed' AND completed_at IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_editorial_tasks_completed_at",
                table: "editorial_tasks");

            migrationBuilder.DropColumn(
                name: "completed_at",
                table: "editorial_tasks");
        }
    }
}

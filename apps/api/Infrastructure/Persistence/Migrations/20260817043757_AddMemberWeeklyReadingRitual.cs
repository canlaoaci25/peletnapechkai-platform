using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberWeeklyReadingRitual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "weekly_reading_goal",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "completed_at",
                table: "article_reading_progress",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_reading_progress_user_completed",
                table: "article_reading_progress",
                columns: new[] { "user_id", "completed_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_reading_progress_user_completed",
                table: "article_reading_progress");

            migrationBuilder.DropColumn(
                name: "weekly_reading_goal",
                table: "users");

            migrationBuilder.DropColumn(
                name: "completed_at",
                table: "article_reading_progress");
        }
    }
}

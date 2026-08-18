using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVisualGenerationLeases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "dead_lettered_at",
                table: "visual_review_tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_failure_code",
                table: "visual_review_tasks",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "lease_expires_at",
                table: "visual_review_tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "lease_owner",
                table: "visual_review_tasks",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "lease_token",
                table: "visual_review_tasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "next_attempt_at",
                table: "visual_review_tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_visual_review_tasks_generation_queue",
                table: "visual_review_tasks",
                columns: new[] { "status", "next_attempt_at", "lease_expires_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_visual_review_tasks_generation_queue",
                table: "visual_review_tasks");

            migrationBuilder.DropColumn(
                name: "dead_lettered_at",
                table: "visual_review_tasks");

            migrationBuilder.DropColumn(
                name: "last_failure_code",
                table: "visual_review_tasks");

            migrationBuilder.DropColumn(
                name: "lease_expires_at",
                table: "visual_review_tasks");

            migrationBuilder.DropColumn(
                name: "lease_owner",
                table: "visual_review_tasks");

            migrationBuilder.DropColumn(
                name: "lease_token",
                table: "visual_review_tasks");

            migrationBuilder.DropColumn(
                name: "next_attempt_at",
                table: "visual_review_tasks");
        }
    }
}

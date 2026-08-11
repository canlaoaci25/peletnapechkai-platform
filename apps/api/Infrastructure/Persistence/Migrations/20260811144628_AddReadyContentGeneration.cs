using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReadyContentGeneration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "auto_seo",
                table: "automation_jobs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "auto_translate",
                table: "automation_jobs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "category_id",
                table: "automation_jobs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "include_images",
                table: "automation_jobs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "requested_article_type",
                table: "automation_jobs",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "generated_by_automation_job_id",
                table: "article_localizations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_article_localizations_generation_job",
                table: "article_localizations",
                column: "generated_by_automation_job_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_article_localizations_generation_job",
                table: "article_localizations");

            migrationBuilder.DropColumn(
                name: "auto_seo",
                table: "automation_jobs");

            migrationBuilder.DropColumn(
                name: "auto_translate",
                table: "automation_jobs");

            migrationBuilder.DropColumn(
                name: "category_id",
                table: "automation_jobs");

            migrationBuilder.DropColumn(
                name: "include_images",
                table: "automation_jobs");

            migrationBuilder.DropColumn(
                name: "requested_article_type",
                table: "automation_jobs");

            migrationBuilder.DropColumn(
                name: "generated_by_automation_job_id",
                table: "article_localizations");
        }
    }
}

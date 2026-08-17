using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVisualRenewalBatchCheckpoint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "automation_job_id",
                table: "visual_review_tasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_visual_review_tasks_batch_status",
                table: "visual_review_tasks",
                columns: new[] { "automation_job_id", "status" });

            migrationBuilder.AddForeignKey(
                name: "FK_visual_review_tasks_automation_jobs_automation_job_id",
                table: "visual_review_tasks",
                column: "automation_job_id",
                principalTable: "automation_jobs",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_visual_review_tasks_automation_jobs_automation_job_id",
                table: "visual_review_tasks");

            migrationBuilder.DropIndex(
                name: "ix_visual_review_tasks_batch_status",
                table: "visual_review_tasks");

            migrationBuilder.DropColumn(
                name: "automation_job_id",
                table: "visual_review_tasks");
        }
    }
}

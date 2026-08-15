using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peletnapechkai.Api.Domain.Automation;

namespace Peletnapechkai.Api.Infrastructure.Persistence.Configurations;

public sealed class AutomationJobConfiguration : IEntityTypeConfiguration<AutomationJob>
{
    public void Configure(EntityTypeBuilder<AutomationJob> builder)
    {
        builder.ToTable("automation_jobs");
        builder.HasKey(job => job.Id);
        builder.Property(job => job.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(job => job.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(40);
        builder.Property(job => job.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(24);
        builder.Property(job => job.TargetLocales).HasColumnName("target_locales").HasColumnType("text[]");
        builder.Property(job => job.TotalItems).HasColumnName("total_items");
        builder.Property(job => job.CompletedItems).HasColumnName("completed_items");
        builder.Property(job => job.FailedItems).HasColumnName("failed_items");
        builder.Property(job => job.CurrentPhase).HasColumnName("current_phase");
        builder.Property(job => job.LastMessage).HasColumnName("last_message").HasMaxLength(2000);
        builder.Property(job => job.ReportText).HasColumnName("report_text");
        builder.Property(job => job.CreatedByUserId).HasColumnName("created_by_user_id");
        builder.Property(job => job.CreatedAt).HasColumnName("created_at");
        builder.Property(job => job.UpdatedAt).HasColumnName("updated_at");
        builder.Property(job => job.CompletedAt).HasColumnName("completed_at");
        builder.Property(job => job.CategoryId).HasColumnName("category_id");
        builder.Property(job => job.RequestedArticleType).HasColumnName("requested_article_type").HasMaxLength(32);
        builder.Property(job => job.IncludeImages).HasColumnName("include_images");
        builder.Property(job => job.AutoTranslate).HasColumnName("auto_translate");
        builder.Property(job => job.AutoSeo).HasColumnName("auto_seo");
        builder.Property(job => job.IsAutomaticallyScheduled).HasColumnName("is_automatically_scheduled");
        builder.HasIndex(job => new { job.Status, job.CreatedAt }).HasDatabaseName("ix_automation_jobs_status_created");
    }
}

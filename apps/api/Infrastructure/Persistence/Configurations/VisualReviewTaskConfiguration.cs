using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peletnapechkai.Api.Domain.Automation;

namespace Peletnapechkai.Api.Infrastructure.Persistence.Configurations;

internal sealed class VisualReviewTaskConfiguration : IEntityTypeConfiguration<VisualReviewTask>
{
    public void Configure(EntityTypeBuilder<VisualReviewTask> b)
    {
        b.ToTable("visual_review_tasks"); b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.ArticleLocalizationId).HasColumnName("article_localization_id");
        b.Property(x => x.CurrentMediaAssetId).HasColumnName("current_media_asset_id");
        b.Property(x => x.QualityScore).HasColumnName("quality_score");
        b.Property(x => x.Risks).HasColumnName("risks").HasMaxLength(1000);
        b.Property(x => x.SectionContext).HasColumnName("section_context").HasMaxLength(600);
        b.Property(x => x.VisualPurpose).HasColumnName("visual_purpose").HasMaxLength(80);
        b.Property(x => x.ProposedPrompt).HasColumnName("proposed_prompt").HasMaxLength(3000);
        b.Property(x => x.NegativePrompt).HasColumnName("negative_prompt").HasMaxLength(1200);
        b.Property(x => x.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(160);
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(24);
        b.Property(x => x.AttemptCount).HasColumnName("attempt_count");
        b.Property(x => x.ReviewerNote).HasColumnName("reviewer_note").HasMaxLength(1000);
        b.Property(x => x.ReviewedByUserId).HasColumnName("reviewed_by_user_id");
        b.Property(x => x.ReviewedAt).HasColumnName("reviewed_at");
        b.Property(x => x.CreatedAt).HasColumnName("created_at"); b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        b.HasIndex(x => x.IdempotencyKey).IsUnique().HasDatabaseName("ux_visual_review_tasks_idempotency");
        b.HasIndex(x => new { x.Status, x.QualityScore, x.CreatedAt }).HasDatabaseName("ix_visual_review_tasks_queue");
        b.HasOne<Domain.Content.ArticleLocalization>().WithMany().HasForeignKey(x => x.ArticleLocalizationId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<Domain.Content.MediaAsset>().WithMany().HasForeignKey(x => x.CurrentMediaAssetId).OnDelete(DeleteBehavior.SetNull);
    }
}

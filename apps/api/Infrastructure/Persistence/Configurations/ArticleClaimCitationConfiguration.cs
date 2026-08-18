using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peletnapechkai.Api.Domain.Content;

namespace Peletnapechkai.Api.Infrastructure.Persistence.Configurations;

public sealed class ArticleClaimCitationConfiguration : IEntityTypeConfiguration<ArticleClaimCitation>
{
    public void Configure(EntityTypeBuilder<ArticleClaimCitation> builder)
    {
        builder.ToTable("article_claim_citations", table =>
        {
            table.HasCheckConstraint("ck_article_claim_citations_claim_not_empty", "length(trim(claim)) > 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.ArticleLocalizationId).HasColumnName("article_localization_id");
        builder.Property(x => x.SourceId).HasColumnName("source_id");
        builder.Property(x => x.Claim).HasColumnName("claim").HasMaxLength(500);
        builder.Property(x => x.Locator).HasColumnName("locator").HasMaxLength(240);
        builder.Property(x => x.ApprovedByUserId).HasColumnName("approved_by_user_id");
        builder.Property(x => x.ApprovedAt).HasColumnName("approved_at");
        builder.HasIndex(x => new { x.ArticleLocalizationId, x.ApprovedAt }).HasDatabaseName("ix_article_claim_citations_article_time");
        builder.HasOne(x => x.ArticleLocalization).WithMany(x => x.ClaimCitations).HasForeignKey(x => x.ArticleLocalizationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Source).WithMany().HasForeignKey(x => x.SourceId).OnDelete(DeleteBehavior.Restrict);
    }
}

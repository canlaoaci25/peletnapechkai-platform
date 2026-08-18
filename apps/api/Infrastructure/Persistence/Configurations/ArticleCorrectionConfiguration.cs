using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peletnapechkai.Api.Domain.Content;

namespace Peletnapechkai.Api.Infrastructure.Persistence.Configurations;

public sealed class ArticleCorrectionConfiguration : IEntityTypeConfiguration<ArticleCorrection>
{
    public void Configure(EntityTypeBuilder<ArticleCorrection> builder)
    {
        builder.ToTable("article_corrections", table =>
        {
            table.HasCheckConstraint("ck_article_corrections_summary", "length(trim(summary)) > 0");
            table.HasCheckConstraint("ck_article_corrections_details", "length(trim(details)) > 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.ArticleLocalizationId).HasColumnName("article_localization_id");
        builder.Property(x => x.Summary).HasColumnName("summary").HasMaxLength(240);
        builder.Property(x => x.Details).HasColumnName("details").HasMaxLength(2000);
        builder.Property(x => x.ApprovedByUserId).HasColumnName("approved_by_user_id");
        builder.Property(x => x.CorrectedAt).HasColumnName("corrected_at");
        builder.HasIndex(x => new { x.ArticleLocalizationId, x.CorrectedAt }).HasDatabaseName("ix_article_corrections_article_date");
        builder.HasOne(x => x.Article).WithMany(x => x.Corrections).HasForeignKey(x => x.ArticleLocalizationId).OnDelete(DeleteBehavior.Cascade);
    }
}

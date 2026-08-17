using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peletnapechkai.Api.Domain.Content;

namespace Peletnapechkai.Api.Infrastructure.Persistence.Configurations;

public sealed class ArticleReadingProgressConfiguration : IEntityTypeConfiguration<ArticleReadingProgress>
{
    public void Configure(EntityTypeBuilder<ArticleReadingProgress> builder)
    {
        builder.ToTable("article_reading_progress");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(item => item.UserId).HasColumnName("user_id");
        builder.Property(item => item.ArticleLocalizationId).HasColumnName("article_localization_id");
        builder.Property(item => item.Percent).HasColumnName("percent");
        builder.Property(item => item.Anchor).HasColumnName("anchor").HasMaxLength(160);
        builder.Property(item => item.LastReadAt).HasColumnName("last_read_at");
        builder.HasIndex(item => new { item.UserId, item.ArticleLocalizationId }).IsUnique().HasDatabaseName("ux_reading_progress_user_article");
        builder.HasIndex(item => new { item.UserId, item.LastReadAt }).HasDatabaseName("ix_reading_progress_user_last_read");
        builder.HasOne(item => item.User).WithMany().HasForeignKey(item => item.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(item => item.ArticleLocalization).WithMany().HasForeignKey(item => item.ArticleLocalizationId).OnDelete(DeleteBehavior.Cascade);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peletnapechkai.Api.Domain.Content;

namespace Peletnapechkai.Api.Infrastructure.Persistence.Configurations;

public sealed class SavedArticleConfiguration : IEntityTypeConfiguration<SavedArticle>
{
    public void Configure(EntityTypeBuilder<SavedArticle> builder)
    {
        builder.ToTable("saved_articles");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(item => item.UserId).HasColumnName("user_id");
        builder.Property(item => item.ArticleLocalizationId).HasColumnName("article_localization_id");
        builder.Property(item => item.SavedAt).HasColumnName("saved_at");
        builder.HasIndex(item => new { item.UserId, item.ArticleLocalizationId })
            .IsUnique().HasDatabaseName("ux_saved_articles_user_article");
        builder.HasIndex(item => new { item.UserId, item.SavedAt })
            .HasDatabaseName("ix_saved_articles_user_saved_at");
        builder.HasOne(item => item.User).WithMany().HasForeignKey(item => item.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(item => item.ArticleLocalization).WithMany().HasForeignKey(item => item.ArticleLocalizationId).OnDelete(DeleteBehavior.Cascade);
    }
}

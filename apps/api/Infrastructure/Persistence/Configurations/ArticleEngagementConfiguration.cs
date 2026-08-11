using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peletnapechkai.Api.Domain.Content;

namespace Peletnapechkai.Api.Infrastructure.Persistence.Configurations;

public sealed class ArticleEngagementConfiguration : IEntityTypeConfiguration<ArticleEngagement>
{
    public void Configure(EntityTypeBuilder<ArticleEngagement> builder)
    {
        builder.ToTable("article_engagements"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.ArticleLocalizationId).HasColumnName("article_localization_id");
        builder.Property(x => x.ViewCount).HasColumnName("view_count");
        builder.Property(x => x.EngagedSeconds).HasColumnName("engaged_seconds");
        builder.Property(x => x.LastViewedAt).HasColumnName("last_viewed_at");
        builder.HasIndex(x => x.ArticleLocalizationId).IsUnique().HasDatabaseName("ux_article_engagements_article");
        builder.HasOne(x => x.ArticleLocalization).WithOne().HasForeignKey<ArticleEngagement>(x => x.ArticleLocalizationId).OnDelete(DeleteBehavior.Cascade);
    }
}

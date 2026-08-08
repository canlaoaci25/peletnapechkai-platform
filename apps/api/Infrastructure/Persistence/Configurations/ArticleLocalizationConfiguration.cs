using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peletnapechkai.Api.Domain.Content;

namespace Peletnapechkai.Api.Infrastructure.Persistence.Configurations;

public sealed class ArticleLocalizationConfiguration : IEntityTypeConfiguration<ArticleLocalization>
{
    public void Configure(EntityTypeBuilder<ArticleLocalization> builder)
    {
        builder.ToTable("article_localizations", table =>
        {
            table.HasCheckConstraint("ck_article_localizations_slug_not_empty", "length(trim(slug)) > 0");
            table.HasCheckConstraint("ck_article_localizations_title_not_empty", "length(trim(title)) > 0");
        });
        builder.HasKey(article => article.Id);

        builder.Property(article => article.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(article => article.ArticleGroupId).HasColumnName("article_group_id");
        builder.Property(article => article.LocaleId).HasColumnName("locale_id");
        builder.Property(article => article.Slug).HasColumnName("slug").HasMaxLength(240);
        builder.Property(article => article.Title).HasColumnName("title").HasMaxLength(180);
        builder.Property(article => article.Summary).HasColumnName("summary").HasMaxLength(500);
        builder.Property(article => article.Body).HasColumnName("body").HasColumnType("text");
        builder.Property(article => article.SeoTitle).HasColumnName("seo_title").HasMaxLength(180);
        builder.Property(article => article.SeoDescription).HasColumnName("seo_description").HasMaxLength(320);
        builder.Property(article => article.CoverMediaAssetId).HasColumnName("cover_media_asset_id");
        builder.Property(article => article.CoverAltText).HasColumnName("cover_alt_text").HasMaxLength(500);
        builder.Property(article => article.CoverCaption).HasColumnName("cover_caption").HasMaxLength(1000);
        builder.Property(article => article.CoverCredit).HasColumnName("cover_credit").HasMaxLength(300);
        builder.Property(article => article.IsSponsored).HasColumnName("is_sponsored");
        builder.Property(article => article.SponsorName).HasColumnName("sponsor_name").HasMaxLength(200);
        builder.Property(article => article.HasAffiliateLinks).HasColumnName("has_affiliate_links");
        builder.Property(article => article.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32);
        builder.Property(article => article.ScheduledAt).HasColumnName("scheduled_at");
        builder.Property(article => article.PublishedAt).HasColumnName("published_at");
        builder.Property(article => article.CreatedAt).HasColumnName("created_at");
        builder.Property(article => article.UpdatedAt).HasColumnName("updated_at").IsConcurrencyToken();

        builder.HasIndex(article => new { article.LocaleId, article.Slug })
            .IsUnique()
            .HasDatabaseName("ux_article_localizations_locale_slug");
        builder.HasIndex(article => new { article.ArticleGroupId, article.LocaleId })
            .IsUnique()
            .HasDatabaseName("ux_article_localizations_group_locale");
        builder.HasIndex(article => new { article.LocaleId, article.Status, article.PublishedAt })
            .HasDatabaseName("ix_article_localizations_publication");

        builder.HasOne(article => article.ArticleGroup)
            .WithMany(group => group.Localizations)
            .HasForeignKey(article => article.ArticleGroupId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(article => article.Locale)
            .WithMany(locale => locale.ArticleLocalizations)
            .HasForeignKey(article => article.LocaleId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(article => article.CoverMediaAsset)
            .WithMany()
            .HasForeignKey(article => article.CoverMediaAssetId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

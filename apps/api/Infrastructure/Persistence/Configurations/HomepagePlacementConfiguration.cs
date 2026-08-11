using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peletnapechkai.Api.Domain.Content;

namespace Peletnapechkai.Api.Infrastructure.Persistence.Configurations;

public sealed class HomepagePlacementConfiguration : IEntityTypeConfiguration<HomepagePlacement>
{
    public void Configure(EntityTypeBuilder<HomepagePlacement> builder)
    {
        builder.ToTable("homepage_placements"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever(); builder.Property(x => x.LocaleId).HasColumnName("locale_id");
        builder.Property(x => x.ArticleLocalizationId).HasColumnName("article_localization_id"); builder.Property(x => x.Section).HasColumnName("section").HasMaxLength(24);
        builder.Property(x => x.Position).HasColumnName("position"); builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.HasIndex(x => new { x.LocaleId, x.Section, x.Position }).IsUnique().HasDatabaseName("ux_homepage_placements_slot");
        builder.HasIndex(x => new { x.LocaleId, x.ArticleLocalizationId }).IsUnique().HasDatabaseName("ux_homepage_placements_article");
        builder.HasOne(x => x.Locale).WithMany().HasForeignKey(x => x.LocaleId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.ArticleLocalization).WithMany().HasForeignKey(x => x.ArticleLocalizationId).OnDelete(DeleteBehavior.Cascade);
    }
}

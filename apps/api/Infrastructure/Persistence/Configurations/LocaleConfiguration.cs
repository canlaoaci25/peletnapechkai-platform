using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peletnapechkai.Api.Domain.Localization;

namespace Peletnapechkai.Api.Infrastructure.Persistence.Configurations;

public sealed class LocaleConfiguration : IEntityTypeConfiguration<Locale>
{
    public void Configure(EntityTypeBuilder<Locale> builder)
    {
        builder.ToTable("locales");
        builder.HasKey(locale => locale.Id);

        builder.Property(locale => locale.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(locale => locale.Code).HasColumnName("code").HasMaxLength(10);
        builder.Property(locale => locale.LanguageCode).HasColumnName("language_code").HasMaxLength(3);
        builder.Property(locale => locale.RegionId).HasColumnName("region_id");
        builder.Property(locale => locale.DisplayName).HasColumnName("display_name").HasMaxLength(100);
        builder.Property(locale => locale.NativeName).HasColumnName("native_name").HasMaxLength(100);
        builder.Property(locale => locale.IsDefault).HasColumnName("is_default");
        builder.Property(locale => locale.IsEnabled).HasColumnName("is_enabled");

        builder.HasIndex(locale => locale.Code).IsUnique().HasDatabaseName("ux_locales_code");
        builder.HasIndex(locale => locale.IsDefault)
            .IsUnique()
            .HasFilter("is_default")
            .HasDatabaseName("ux_locales_single_default");

        builder.HasOne(locale => locale.Region)
            .WithMany(region => region.Locales)
            .HasForeignKey(locale => locale.RegionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(SeedData.Locales);
    }
}

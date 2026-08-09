using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peletnapechkai.Api.Domain.Localization;

namespace Peletnapechkai.Api.Infrastructure.Persistence.Configurations;

public sealed class LocaleCountryConfiguration : IEntityTypeConfiguration<LocaleCountry>
{
    public void Configure(EntityTypeBuilder<LocaleCountry> builder)
    {
        builder.ToTable("locale_countries");
        builder.HasKey(item => new { item.LocaleId, item.CountryId });
        builder.Property(item => item.LocaleId).HasColumnName("locale_id");
        builder.Property(item => item.CountryId).HasColumnName("country_id");
        builder.Property(item => item.IsRequired).HasColumnName("is_required");
        builder.Property(item => item.IsEnabled).HasColumnName("is_enabled");
        builder.HasOne(item => item.Locale).WithMany(locale => locale.Countries).HasForeignKey(item => item.LocaleId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(item => item.Country).WithMany(country => country.LocaleCountries).HasForeignKey(item => item.CountryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(item => new { item.CountryId, item.IsEnabled }).HasDatabaseName("ix_locale_countries_country_enabled");
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peletnapechkai.Api.Domain.Localization;

namespace Peletnapechkai.Api.Infrastructure.Persistence.Configurations;

public sealed class RegionConfiguration : IEntityTypeConfiguration<Region>
{
    public void Configure(EntityTypeBuilder<Region> builder)
    {
        builder.ToTable("regions");
        builder.HasKey(region => region.Id);

        builder.Property(region => region.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(region => region.Code).HasColumnName("code").HasMaxLength(2);
        builder.Property(region => region.Name).HasColumnName("name").HasMaxLength(100);
        builder.Property(region => region.CurrencyCode).HasColumnName("currency_code").HasMaxLength(3);
        builder.Property(region => region.IsEnabled).HasColumnName("is_enabled");

        builder.HasIndex(region => region.Code).IsUnique().HasDatabaseName("ux_regions_code");
        builder.HasData(SeedData.Regions);
    }
}

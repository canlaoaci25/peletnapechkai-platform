using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peletnapechkai.Api.Domain.Content;

namespace Peletnapechkai.Api.Infrastructure.Persistence.Configurations;

public sealed class WebVitalSampleConfiguration : IEntityTypeConfiguration<WebVitalSample>
{
    public void Configure(EntityTypeBuilder<WebVitalSample> builder)
    {
        builder.ToTable("web_vital_samples", table =>
        {
            table.HasCheckConstraint("ck_web_vitals_metric", "metric IN ('LCP','CLS','INP')");
            table.HasCheckConstraint("ck_web_vitals_route", "route IN ('home','article','category','search','other')");
            table.HasCheckConstraint("ck_web_vitals_viewport", "viewport IN ('mobile','tablet','desktop')");
            table.HasCheckConstraint("ck_web_vitals_locale", "locale IN ('tr-TR','en-US','de-DE','fr-FR')");
            table.HasCheckConstraint("ck_web_vitals_value", "value >= 0 AND ((metric = 'CLS' AND value <= 5) OR (metric IN ('LCP','INP') AND value <= 60000))");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.Locale).HasColumnName("locale").HasMaxLength(10);
        builder.Property(x => x.Route).HasColumnName("route").HasMaxLength(20);
        builder.Property(x => x.Viewport).HasColumnName("viewport").HasMaxLength(10);
        builder.Property(x => x.Metric).HasColumnName("metric").HasMaxLength(3);
        builder.Property(x => x.Value).HasColumnName("value");
        builder.Property(x => x.MeasuredAt).HasColumnName("measured_at");
        builder.HasIndex(x => new { x.MeasuredAt, x.Locale, x.Viewport, x.Metric }).HasDatabaseName("ix_web_vitals_window");
    }
}

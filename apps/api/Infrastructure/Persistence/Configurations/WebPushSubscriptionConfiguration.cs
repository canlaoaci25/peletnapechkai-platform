using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peletnapechkai.Api.Domain.Content;

namespace Peletnapechkai.Api.Infrastructure.Persistence.Configurations;

public sealed class WebPushSubscriptionConfiguration : IEntityTypeConfiguration<WebPushSubscription>
{
    public void Configure(EntityTypeBuilder<WebPushSubscription> builder)
    {
        builder.ToTable("web_push_subscriptions", table => {
            table.HasCheckConstraint("ck_web_push_quiet_start", "quiet_starts_at >= 0 AND quiet_starts_at <= 23");
            table.HasCheckConstraint("ck_web_push_quiet_end", "quiet_ends_at >= 0 AND quiet_ends_at <= 23");
        });
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(item => item.UserId).HasColumnName("user_id");
        builder.Property(item => item.Endpoint).HasColumnName("endpoint").HasMaxLength(2048);
        builder.Property(item => item.P256dh).HasColumnName("p256dh").HasMaxLength(512);
        builder.Property(item => item.Auth).HasColumnName("auth").HasMaxLength(256);
        builder.Property(item => item.Locale).HasColumnName("locale").HasMaxLength(5);
        builder.Property(item => item.QuietStartsAt).HasColumnName("quiet_starts_at");
        builder.Property(item => item.QuietEndsAt).HasColumnName("quiet_ends_at");
        builder.Property(item => item.IsEnabled).HasColumnName("is_enabled");
        builder.Property(item => item.CreatedAt).HasColumnName("created_at");
        builder.Property(item => item.UpdatedAt).HasColumnName("updated_at");
        builder.HasIndex(item => item.Endpoint).IsUnique().HasDatabaseName("ux_web_push_subscriptions_endpoint");
        builder.HasIndex(item => new { item.UserId, item.IsEnabled }).HasDatabaseName("ix_web_push_subscriptions_user_enabled");
        builder.HasOne(item => item.User).WithMany().HasForeignKey(item => item.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

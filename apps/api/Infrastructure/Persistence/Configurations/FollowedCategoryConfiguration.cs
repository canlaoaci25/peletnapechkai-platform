using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peletnapechkai.Api.Domain.Content;

namespace Peletnapechkai.Api.Infrastructure.Persistence.Configurations;

public sealed class FollowedCategoryConfiguration : IEntityTypeConfiguration<FollowedCategory>
{
    public void Configure(EntityTypeBuilder<FollowedCategory> builder)
    {
        builder.ToTable("followed_categories");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(item => item.UserId).HasColumnName("user_id");
        builder.Property(item => item.CategoryId).HasColumnName("category_id");
        builder.Property(item => item.FollowedAt).HasColumnName("followed_at");
        builder.HasIndex(item => new { item.UserId, item.CategoryId }).IsUnique().HasDatabaseName("ux_followed_categories_user_category");
        builder.HasIndex(item => new { item.UserId, item.FollowedAt }).HasDatabaseName("ix_followed_categories_user_followed_at");
        builder.HasOne(item => item.User).WithMany().HasForeignKey(item => item.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(item => item.Category).WithMany().HasForeignKey(item => item.CategoryId).OnDelete(DeleteBehavior.Cascade);
    }
}

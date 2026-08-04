using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peletnapechkai.Api.Domain.Content;

namespace Peletnapechkai.Api.Infrastructure.Persistence.Configurations;

public sealed class ArticleGroupConfiguration : IEntityTypeConfiguration<ArticleGroup>
{
    public void Configure(EntityTypeBuilder<ArticleGroup> builder)
    {
        builder.ToTable("article_groups");
        builder.HasKey(group => group.Id);

        builder.Property(group => group.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(group => group.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(24);
        builder.Property(group => group.CreatedAt).HasColumnName("created_at");
        builder.Property(group => group.UpdatedAt).HasColumnName("updated_at");
    }
}

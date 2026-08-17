using Microsoft.EntityFrameworkCore; using Microsoft.EntityFrameworkCore.Metadata.Builders; using Peletnapechkai.Api.Domain.Localization; using Peletnapechkai.Api.Domain.Content;
namespace Peletnapechkai.Api.Infrastructure.Persistence.Configurations;
internal sealed class LocalizationAssignmentConfiguration : IEntityTypeConfiguration<LocalizationAssignment>
{
    public void Configure(EntityTypeBuilder<LocalizationAssignment> b)
    {
        b.ToTable("localization_assignments"); b.HasKey(x => x.Id); b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.ArticleGroupId).HasColumnName("article_group_id"); b.Property(x => x.TargetLocaleId).HasColumnName("target_locale_id"); b.Property(x => x.AssigneeUserId).HasColumnName("assignee_user_id");
        b.Property(x => x.DueAt).HasColumnName("due_at"); b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20); b.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id"); b.Property(x => x.CreatedAt).HasColumnName("created_at"); b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        b.HasIndex(x => new { x.ArticleGroupId, x.TargetLocaleId }).IsUnique().HasDatabaseName("ux_localization_assignments_group_locale");
        b.HasIndex(x => new { x.AssigneeUserId, x.Status, x.DueAt }).HasDatabaseName("ix_localization_assignments_owner_sla");
        b.HasOne<ArticleGroup>().WithMany().HasForeignKey(x => x.ArticleGroupId).OnDelete(DeleteBehavior.Cascade); b.HasOne<Locale>().WithMany().HasForeignKey(x => x.TargetLocaleId).OnDelete(DeleteBehavior.Restrict);
    }
}

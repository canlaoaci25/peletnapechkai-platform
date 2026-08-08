using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peletnapechkai.Api.Domain.Knowledge;

namespace Peletnapechkai.Api.Infrastructure.Persistence.Configurations;

internal sealed class KnowledgeArticleLinkConfiguration:IEntityTypeConfiguration<KnowledgeArticleLink>
{
    public void Configure(EntityTypeBuilder<KnowledgeArticleLink> b)
    {
        b.ToTable("knowledge_article_links"); b.HasKey(x=>x.Id); b.Property(x=>x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x=>x.KnowledgeCandidateId).HasColumnName("knowledge_candidate_id"); b.Property(x=>x.ArticleLocalizationId).HasColumnName("article_localization_id");
        b.Property(x=>x.Purpose).HasColumnName("purpose").HasConversion<string>().HasMaxLength(32); b.Property(x=>x.Note).HasColumnName("note").HasMaxLength(1000);
        b.Property(x=>x.ReviewDueAt).HasColumnName("review_due_at"); b.Property(x=>x.LastVerifiedAt).HasColumnName("last_verified_at"); b.Property(x=>x.LastVerifiedByUserId).HasColumnName("last_verified_by_user_id");
        b.Property(x=>x.CreatedByUserId).HasColumnName("created_by_user_id"); b.Property(x=>x.CreatedAt).HasColumnName("created_at");
        b.HasOne(x=>x.Candidate).WithMany().HasForeignKey(x=>x.KnowledgeCandidateId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x=>x.Article).WithMany().HasForeignKey(x=>x.ArticleLocalizationId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x=>new{x.KnowledgeCandidateId,x.ArticleLocalizationId}).IsUnique().HasDatabaseName("ux_knowledge_article_link");
        b.HasIndex(x=>x.ReviewDueAt).HasDatabaseName("ix_knowledge_article_review_due");
    }
}

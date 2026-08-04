using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peletnapechkai.Api.Domain.Auditing;
using Peletnapechkai.Api.Domain.Content;

namespace Peletnapechkai.Api.Infrastructure.Persistence.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories", table =>
        {
            table.HasCheckConstraint("ck_categories_slug_not_empty", "length(trim(slug)) > 0");
            table.HasCheckConstraint("ck_categories_name_not_empty", "length(trim(name)) > 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.LocaleId).HasColumnName("locale_id");
        builder.Property(x => x.Slug).HasColumnName("slug").HasMaxLength(160);
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(160);
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.HasOne(x => x.Locale).WithMany().HasForeignKey(x => x.LocaleId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.LocaleId, x.Slug }).IsUnique().HasDatabaseName("ux_categories_locale_slug");
    }
}

internal sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("tags", table =>
        {
            table.HasCheckConstraint("ck_tags_slug_not_empty", "length(trim(slug)) > 0");
            table.HasCheckConstraint("ck_tags_name_not_empty", "length(trim(name)) > 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.LocaleId).HasColumnName("locale_id");
        builder.Property(x => x.Slug).HasColumnName("slug").HasMaxLength(160);
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(160);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.HasOne(x => x.Locale).WithMany().HasForeignKey(x => x.LocaleId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.LocaleId, x.Slug }).IsUnique().HasDatabaseName("ux_tags_locale_slug");
    }
}

internal sealed class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.ToTable("authors");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.Slug).HasColumnName("slug").HasMaxLength(160);
        builder.Property(x => x.DisplayName).HasColumnName("display_name").HasMaxLength(160);
        builder.Property(x => x.Bio).HasColumnName("bio").HasMaxLength(2000);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.HasIndex(x => x.Slug).IsUnique().HasDatabaseName("ux_authors_slug");
    }
}

internal sealed class SourceConfiguration : IEntityTypeConfiguration<Source>
{
    public void Configure(EntityTypeBuilder<Source> builder)
    {
        builder.ToTable("sources");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200);
        builder.Property(x => x.Url).HasColumnName("url").HasMaxLength(2048);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.HasIndex(x => x.Url).IsUnique().HasDatabaseName("ux_sources_url");
    }
}

internal sealed class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> builder)
    {
        builder.ToTable("media_assets", table => table.HasCheckConstraint("ck_media_assets_byte_length_positive", "byte_length > 0"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.StorageKey).HasColumnName("storage_key").HasMaxLength(500);
        builder.Property(x => x.FileName).HasColumnName("file_name").HasMaxLength(255);
        builder.Property(x => x.ContentType).HasColumnName("content_type").HasMaxLength(127);
        builder.Property(x => x.ByteLength).HasColumnName("byte_length");
        builder.Property(x => x.Width).HasColumnName("width");
        builder.Property(x => x.Height).HasColumnName("height");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.HasIndex(x => x.StorageKey).IsUnique().HasDatabaseName("ux_media_assets_storage_key");
    }
}

internal sealed class ArticleRevisionConfiguration : IEntityTypeConfiguration<ArticleRevision>
{
    public void Configure(EntityTypeBuilder<ArticleRevision> builder)
    {
        builder.ToTable("article_revisions", table => table.HasCheckConstraint("ck_article_revisions_number_positive", "number > 0"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.ArticleLocalizationId).HasColumnName("article_localization_id");
        builder.Property(x => x.Number).HasColumnName("number");
        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(300);
        builder.Property(x => x.Summary).HasColumnName("summary").HasMaxLength(1000);
        builder.Property(x => x.Body).HasColumnName("body");
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.HasOne(x => x.ArticleLocalization).WithMany(x => x.Revisions).HasForeignKey(x => x.ArticleLocalizationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.ArticleLocalizationId, x.Number }).IsUnique().HasDatabaseName("ux_article_revisions_article_number");
    }
}

internal sealed class SeoMetadataConfiguration : IEntityTypeConfiguration<SeoMetadata>
{
    public void Configure(EntityTypeBuilder<SeoMetadata> builder)
    {
        builder.ToTable("seo_metadata");
        builder.HasKey(x => x.ArticleLocalizationId);
        builder.Property(x => x.ArticleLocalizationId).HasColumnName("article_localization_id").ValueGeneratedNever();
        builder.Property(x => x.CanonicalUrl).HasColumnName("canonical_url").HasMaxLength(2048);
        builder.Property(x => x.OpenGraphTitle).HasColumnName("open_graph_title").HasMaxLength(300);
        builder.Property(x => x.OpenGraphDescription).HasColumnName("open_graph_description").HasMaxLength(500);
        builder.Property(x => x.RobotsDirective).HasColumnName("robots_directive").HasMaxLength(100);
        builder.Property(x => x.StructuredDataJson).HasColumnName("structured_data_json").HasColumnType("jsonb");
        builder.HasOne(x => x.ArticleLocalization).WithOne(x => x.SeoMetadata).HasForeignKey<SeoMetadata>(x => x.ArticleLocalizationId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.ActorUserId).HasColumnName("actor_user_id");
        builder.Property(x => x.Action).HasColumnName("action").HasMaxLength(100);
        builder.Property(x => x.EntityType).HasColumnName("entity_type").HasMaxLength(160);
        builder.Property(x => x.EntityId).HasColumnName("entity_id");
        builder.Property(x => x.DetailsJson).HasColumnName("details_json").HasColumnType("jsonb");
        builder.Property(x => x.OccurredAt).HasColumnName("occurred_at");
        builder.HasIndex(x => new { x.EntityType, x.EntityId, x.OccurredAt }).HasDatabaseName("ix_audit_logs_entity_time");
    }
}

internal sealed class PublishingRelationshipsConfiguration : IEntityTypeConfiguration<ArticleLocalization>
{
    public void Configure(EntityTypeBuilder<ArticleLocalization> builder)
    {
        builder.HasMany(x => x.Categories).WithMany(x => x.Articles).UsingEntity("article_categories",
            right => right.HasOne(typeof(Category)).WithMany().HasForeignKey("category_id").OnDelete(DeleteBehavior.Cascade),
            left => left.HasOne(typeof(ArticleLocalization)).WithMany().HasForeignKey("article_localization_id").OnDelete(DeleteBehavior.Cascade),
            join =>
            {
                join.ToTable("article_categories");
                join.HasKey("article_localization_id", "category_id");
            });
        builder.HasMany(x => x.Tags).WithMany(x => x.Articles).UsingEntity("article_tags",
            right => right.HasOne(typeof(Tag)).WithMany().HasForeignKey("tag_id").OnDelete(DeleteBehavior.Cascade),
            left => left.HasOne(typeof(ArticleLocalization)).WithMany().HasForeignKey("article_localization_id").OnDelete(DeleteBehavior.Cascade),
            join =>
            {
                join.ToTable("article_tags");
                join.HasKey("article_localization_id", "tag_id");
            });
    }
}

internal sealed class ArticleGroupRelationshipsConfiguration : IEntityTypeConfiguration<ArticleGroup>
{
    public void Configure(EntityTypeBuilder<ArticleGroup> builder)
    {
        ConfigureJoin(builder, x => x.Authors, "article_authors", "author_id");
        ConfigureJoin(builder, x => x.Sources, "article_sources", "source_id");
        ConfigureJoin(builder, x => x.MediaAssets, "article_media_assets", "media_asset_id");
    }

    private static void ConfigureJoin<TRelated>(EntityTypeBuilder<ArticleGroup> builder, System.Linq.Expressions.Expression<Func<ArticleGroup, IEnumerable<TRelated>?>> navigation, string tableName, string relatedKey)
        where TRelated : class
    {
        builder.HasMany(navigation).WithMany().UsingEntity(tableName,
            right => right.HasOne(typeof(TRelated)).WithMany().HasForeignKey(relatedKey).OnDelete(DeleteBehavior.Cascade),
            left => left.HasOne(typeof(ArticleGroup)).WithMany().HasForeignKey("article_group_id").OnDelete(DeleteBehavior.Cascade),
            join =>
            {
                join.ToTable(tableName);
                join.HasKey("article_group_id", relatedKey);
            });
    }
}

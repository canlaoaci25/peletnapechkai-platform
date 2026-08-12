using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Domain.Content;
using Peletnapechkai.Api.Infrastructure.Persistence;

namespace Peletnapechkai.Api.Endpoints;

public static class PublicContentEndpoints
{
    public static IEndpointRouteBuilder MapPublicContentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/public").WithTags("Public content");
        group.MapGet("/{locale}/articles", ListAsync);
        group.MapGet("/{locale}/articles/search", SearchAsync);
        group.MapGet("/{locale}/articles/{slug}", GetAsync);
        group.MapGet("/{locale}/archives", ListArchivesAsync);
        group.MapGet("/{locale}/archives/{kind}/{slug}", GetArchiveAsync);
        group.MapGet("/media/{assetId:guid}", GetMediaAsync);
        return endpoints;
    }

    private static async Task<IResult> GetMediaAsync(Guid assetId, PublishingDbContext database, IConfiguration configuration, CancellationToken token)
    {
        var isPublishedMedia = await database.ArticleLocalizations.AsNoTracking()
            .AnyAsync(article => article.Status == PublicationStatus.Published &&
                (article.CoverMediaAssetId == assetId || article.Body.Contains(assetId.ToString())), token);
        if (!isPublishedMedia) return Results.NotFound();
        var asset = await database.MediaAssets.AsNoTracking().Where(item => item.Id == assetId)
            .Select(item => new { StorageKey=item.OptimizedStorageKey??item.StorageKey, ContentType=item.OptimizedStorageKey==null?item.ContentType:"image/webp", item.CreatedAt }).SingleOrDefaultAsync(token);
        if (asset is null) return Results.NotFound();
        var root = Path.GetFullPath(configuration["Media:StoragePath"] ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "BOECL", "Media"));
        var path = Path.GetFullPath(Path.Combine(root, asset.StorageKey.Replace('/', Path.DirectorySeparatorChar)));
        if (!path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || !File.Exists(path)) return Results.NotFound();
        return Results.File(path, asset.ContentType, lastModified: asset.CreatedAt, enableRangeProcessing: true);
    }

    private static async Task<IResult> ListArchivesAsync(string locale, PublishingDbContext database, CancellationToken token)
    {
        var categories = await database.Categories.AsNoTracking().Where(item => item.Locale.Code == locale && item.Articles.Any(article => article.Status == PublicationStatus.Published)).OrderBy(item => item.Name).Select(item => new { item.Slug, title = item.Name }).ToListAsync(token);
        var tags = await database.Tags.AsNoTracking().Where(item => item.Locale.Code == locale && item.Articles.Any(article => article.Status == PublicationStatus.Published)).OrderBy(item => item.Name).Select(item => new { item.Slug, title = item.Name }).ToListAsync(token);
        var authors = await database.Authors.AsNoTracking().Where(item => database.ArticleLocalizations.Any(article => article.Locale.Code == locale && article.Status == PublicationStatus.Published && article.ArticleGroup.Authors.Any(author => author.Id == item.Id))).OrderBy(item => item.DisplayName).Select(item => new { item.Slug, title = item.DisplayName }).ToListAsync(token);
        return Results.Ok(new { categories, tags, authors });
    }

    private static async Task<IResult> GetArchiveAsync(string locale, string kind, string slug, PublishingDbContext database, int? limit, CancellationToken token)
    {
        var take = Math.Clamp(limit ?? 24, 1, 50);
        var query = database.ArticleLocalizations.AsNoTracking().Where(article => article.Locale.Code == locale && article.Locale.IsEnabled && article.Status == PublicationStatus.Published);
        string? title;
        string? description = null;
        switch (kind)
        {
            case "categories":
                var category = await database.Categories.AsNoTracking().Where(item => item.Locale.Code == locale && item.Slug == slug).Select(item => new { item.Name, item.Description }).SingleOrDefaultAsync(token);
                if (category is null) return Results.NotFound();
                title = category.Name; description = category.Description;
                query = query.Where(article => article.Categories.Any(item => item.Slug == slug));
                break;
            case "tags":
                title = await database.Tags.AsNoTracking().Where(item => item.Locale.Code == locale && item.Slug == slug).Select(item => item.Name).SingleOrDefaultAsync(token);
                if (title is null) return Results.NotFound();
                query = query.Where(article => article.Tags.Any(item => item.Slug == slug));
                break;
            case "authors":
                var author = await database.Authors.AsNoTracking().Where(item => item.Slug == slug).Select(item => new { item.DisplayName, item.Bio }).SingleOrDefaultAsync(token);
                if (author is null) return Results.NotFound();
                title = author.DisplayName; description = author.Bio;
                query = query.Where(article => article.ArticleGroup.Authors.Any(item => item.Slug == slug));
                break;
            default:
                return Results.NotFound();
        }
        var articles = await query.OrderByDescending(article => article.PublishedAt).Take(take).Select(article => new { article.ArticleGroupId, article.Slug, article.Title, article.Summary, type = article.ArticleGroup.Type.ToString(), article.PublishedAt, article.UpdatedAt, cover = article.CoverMediaAssetId == null ? null : new { url = "/api/media/" + article.CoverMediaAssetId + "?v=" + article.CoverMediaAsset!.OptimizedByteLength, altText = article.CoverAltText } }).ToListAsync(token);
        return Results.Ok(new { kind, slug, title, description, articles });
    }

    private static async Task<IResult> SearchAsync(string locale, string? q, PublishingDbContext database, int? limit, CancellationToken token)
    {
        var term = q?.Trim();
        if (string.IsNullOrWhiteSpace(term) || term.Length < 2) return Results.Ok(Array.Empty<object>());
        var escaped = term.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
        var pattern = $"%{escaped}%";
        var take = Math.Clamp(limit ?? 20, 1, 50);
        var articles = await database.ArticleLocalizations.AsNoTracking()
            .Where(article => article.Locale.Code == locale && article.Locale.IsEnabled && article.Status == PublicationStatus.Published)
            .Where(article => EF.Functions.ILike(article.Title, pattern, "\\") || EF.Functions.ILike(article.Summary, pattern, "\\") || EF.Functions.ILike(article.Body, pattern, "\\"))
            .OrderByDescending(article => article.PublishedAt)
            .Take(take)
            .Select(article => new { article.ArticleGroupId, article.Slug, article.Title, article.Summary, type = article.ArticleGroup.Type.ToString(), article.PublishedAt, article.UpdatedAt, cover = article.CoverMediaAssetId == null ? null : new { url = "/api/media/" + article.CoverMediaAssetId + "?v=" + article.CoverMediaAsset!.OptimizedByteLength, altText = article.CoverAltText } })
            .ToListAsync(token);
        return Results.Ok(articles);
    }

    private static async Task<IResult> ListAsync(string locale, PublishingDbContext database, int? limit, CancellationToken token)
    {
        var take = Math.Clamp(limit ?? 12, 1, 1000);
        var articles = await database.ArticleLocalizations.AsNoTracking()
            .Where(article => article.Locale.Code == locale && article.Locale.IsEnabled && article.Status == PublicationStatus.Published)
            .OrderByDescending(article => article.PublishedAt)
            .Take(take)
            .Select(article => new
            {
                article.ArticleGroupId,
                article.Slug,
                article.Title,
                article.Summary,
                type = article.ArticleGroup.Type.ToString(),
                article.PublishedAt,
                article.UpdatedAt,
                cover = article.CoverMediaAssetId == null ? null : new { url = "/api/media/" + article.CoverMediaAssetId + "?v=" + article.CoverMediaAsset!.OptimizedByteLength, altText = article.CoverAltText }
            })
            .ToListAsync(token);
        return Results.Ok(articles);
    }

    private static async Task<IResult> GetAsync(string locale, string slug, PublishingDbContext database, CancellationToken token)
    {
        var article = await database.ArticleLocalizations.AsNoTracking()
            .Where(item => item.Locale.Code == locale && item.Locale.IsEnabled && item.Slug == slug && item.Status == PublicationStatus.Published)
            .Select(item => new
            {
                item.Slug,
                item.Title,
                item.Summary,
                item.Body,
                item.SeoTitle,
                item.SeoDescription,
                item.IsSponsored,
                item.SponsorName,
                item.HasAffiliateLinks,
                cover = item.CoverMediaAssetId == null ? null : new { url = "/api/media/" + item.CoverMediaAssetId + "?v=" + item.CoverMediaAsset!.OptimizedByteLength, altText = item.CoverAltText, caption = item.CoverCaption, credit = item.CoverCredit },
                type = item.ArticleGroup.Type.ToString(),
                item.PublishedAt,
                item.UpdatedAt,
                categories=item.Categories.Select(x=>new {x.Slug,x.Name}).OrderBy(x=>x.Name).ToArray(),
                tags=item.Tags.Select(x=>new {x.Slug,x.Name}).OrderBy(x=>x.Name).ToArray(),
                authors=item.ArticleGroup.Authors.Select(x=>new {x.Slug,x.DisplayName}).OrderBy(x=>x.DisplayName).ToArray(),
                sources=item.ArticleGroup.Sources.Select(x=>new {x.Name,x.Url}).OrderBy(x=>x.Name).ToArray(),
                translations = item.ArticleGroup.Localizations
                    .Where(translation => translation.Status == PublicationStatus.Published && translation.Locale.IsEnabled)
                    .Select(translation => new { locale = translation.Locale.Code, translation.Slug })
                    .OrderBy(translation => translation.locale)
                    .ToArray()
            })
            .SingleOrDefaultAsync(token);
        return article is null ? Results.NotFound() : Results.Ok(article);
    }
}

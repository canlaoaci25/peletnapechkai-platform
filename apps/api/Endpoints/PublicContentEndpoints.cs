using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Features.Search;
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
        group.MapGet("/{locale}/sources", ListSourcesAsync);
        group.MapGet("/{locale}/sources/{domain}", GetSourceArchiveAsync);
        group.MapGet("/media/{assetId:guid}", GetMediaAsync);
        return endpoints;
    }

    private static async Task<IResult> ListSourcesAsync(string locale, PublishingDbContext database, CancellationToken token)
    {
        var totalArticles = await database.ArticleLocalizations.AsNoTracking()
            .CountAsync(article => article.Locale.Code == locale && article.Locale.IsEnabled && article.Status == PublicationStatus.Published, token);
        var rows = await database.ArticleLocalizations.AsNoTracking()
            .Where(article => article.Locale.Code == locale && article.Locale.IsEnabled && article.Status == PublicationStatus.Published)
            .SelectMany(article => article.ArticleGroup.Sources.Select(source => new { article.Id, source.Name, source.Url, source.Kind, source.LastReviewedAt, article.PublishedAt }))
            .ToListAsync(token);
        var now = DateTimeOffset.UtcNow;
        var validRows = rows.Select(row => new { Row = row, Domain = SourceArchivePolicy.GetCanonicalDomain(row.Url) })
            .Where(item => item.Domain is not null).ToArray();
        var sources = validRows
            .GroupBy(item => item.Domain!)
            .Select(group => new { domain = group.Key, sourceName = group.GroupBy(item => item.Row.Name).OrderByDescending(names => names.Count()).ThenBy(names => names.Key).First().Key,
                kind = group.Where(item=>item.Row.Kind!=SourceKind.Unclassified).GroupBy(item=>item.Row.Kind).OrderByDescending(kinds=>kinds.Count()).Select(kinds=>kinds.Key.ToString()).FirstOrDefault() ?? SourceKind.Unclassified.ToString(), lastReviewedAt=group.Max(item=>item.Row.LastReviewedAt),
                reviewState = SourceTrustPolicy.GetReviewState(group.OrderByDescending(item => item.Row.LastReviewedAt).First().Row.Kind, group.Max(item=>item.Row.LastReviewedAt), now).ToString(),
                articleCount = group.Select(item => item.Row.Id).Distinct().Count(), citationCount = group.Count(), latestCitationAt = group.Max(item => item.Row.PublishedAt) })
            .OrderByDescending(item => item.articleCount).ThenBy(item => item.domain).ToArray();
        var articlesWithSources = validRows.GroupBy(item => item.Row.Id).ToArray();
        return Results.Ok(new {
            totalArticles,
            sourcedArticleCount = articlesWithSources.Length,
            multiDomainArticleCount = articlesWithSources.Count(article => article.Select(item => item.Domain).Distinct().Count() >= 2),
            reviewedSourceCount = sources.Count(source => source.reviewState != SourceReviewState.Unclassified.ToString()),
            currentReviewCount = sources.Count(source => source.reviewState == SourceReviewState.Current.ToString()),
            totalSources = sources.Length,
            totalCitations = sources.Sum(item => item.citationCount),
            sources
        });
    }

    private static async Task<IResult> GetSourceArchiveAsync(string locale, string domain, PublishingDbContext database, CancellationToken token)
    {
        domain = domain.Trim().ToLowerInvariant();
        if (!SourceArchivePolicy.IsValidDomain(domain)) return Results.NotFound();
        var rows = await database.ArticleLocalizations.AsNoTracking()
            .Where(article => article.Locale.Code == locale && article.Locale.IsEnabled && article.Status == PublicationStatus.Published)
            .Where(article => article.ArticleGroup.Sources.Count > 0)
            .Select(article => new { article.Slug, article.Title, article.Summary, type = article.ArticleGroup.Type.ToString(), article.PublishedAt, article.UpdatedAt,
                cover = article.CoverMediaAssetId == null ? null : new { url = "/api/media/" + article.CoverMediaAssetId + "?v=" + article.CoverMediaAsset!.OptimizedByteLength, altText = article.CoverAltText, article.CoverMediaAsset.FocalX, article.CoverMediaAsset.FocalY },
                sources = article.ArticleGroup.Sources.Select(source => new { source.Name, source.Url, source.Kind, source.LastReviewedAt }).ToArray() })
            .OrderByDescending(article => article.PublishedAt).ToListAsync(token);
        var matches = rows.Select(row => new { Row = row, Sources = row.sources.Where(source => SourceArchivePolicy.GetCanonicalDomain(source.Url) == domain).ToArray() })
            .Where(item => item.Sources.Length > 0).ToArray();
        if (matches.Length == 0) return Results.NotFound();
        var names = matches.SelectMany(item => item.Sources).GroupBy(source => source.Name).OrderByDescending(group => group.Count()).ThenBy(group => group.Key).Select(group => group.Key).Take(4).ToArray();
        var reviewed=matches.SelectMany(item=>item.Sources).Where(source=>source.Kind!=SourceKind.Unclassified).OrderByDescending(source=>source.LastReviewedAt).FirstOrDefault();
        return Results.Ok(new { domain, names, kind=(reviewed?.Kind??SourceKind.Unclassified).ToString(), lastReviewedAt=reviewed?.LastReviewedAt, articleCount = matches.Length, citationCount = matches.Sum(item => item.Sources.Length), latestCitationAt = matches.Max(item => item.Row.PublishedAt),
            articles = matches.Select(item => item.Row).ToArray() });
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
        var categories = await database.Categories.AsNoTracking()
            .Where(item => item.Locale.Code == locale && item.Articles.Any(article => article.Status == PublicationStatus.Published))
            .OrderByDescending(item => item.Articles.Count(article => article.Status == PublicationStatus.Published))
            .ThenBy(item => item.Name)
            .Select(item => new
            {
                item.Id,
                item.Slug,
                title = item.Name,
                item.Description,
                parent = item.ParentCategory == null ? null : new { item.ParentCategory.Slug, title = item.ParentCategory.Name },
                translationKey = item.SourceCategoryId ?? item.Id,
                children = item.Children
                    .Where(child => child.Articles.Any(article => article.Status == PublicationStatus.Published))
                    .OrderByDescending(child => child.Articles.Count(article => article.Status == PublicationStatus.Published))
                    .ThenBy(child => child.Name)
                    .Select(child => new { child.Slug, title = child.Name, articleCount = child.Articles.Count(article => article.Status == PublicationStatus.Published) })
                    .ToArray(),
                articleCount = item.Articles.Count(article => article.Status == PublicationStatus.Published),
                featured = item.Articles
                    .Where(article => article.Status == PublicationStatus.Published)
                    .OrderByDescending(article => article.PublishedAt)
                    .Take(3)
                    .Select(article => new { article.ArticleGroupId, article.Slug, article.Title, article.Summary, type = article.ArticleGroup.Type.ToString(), article.PublishedAt, article.UpdatedAt, cover = article.CoverMediaAssetId == null ? null : new { url = "/api/media/" + article.CoverMediaAssetId + "?v=" + article.CoverMediaAsset!.OptimizedByteLength, altText = article.CoverAltText, article.CoverMediaAsset!.FocalX, article.CoverMediaAsset.FocalY } })
                    .ToArray()
            }).ToListAsync(token);
        var tags = await database.Tags.AsNoTracking()
            .Where(item => item.Locale.Code == locale && item.Articles.Any(article => article.Status == PublicationStatus.Published))
            .OrderByDescending(item => item.Articles.Count(article => article.Status == PublicationStatus.Published))
            .ThenBy(item => item.Name)
            .Select(item => new
            {
                item.Slug,
                title = item.Name,
                translationKey = item.SourceTagId ?? item.Id,
                articleCount = item.Articles.Count(article => article.Status == PublicationStatus.Published)
            }).ToListAsync(token);
        var authors = await database.Authors.AsNoTracking().Where(item => database.ArticleLocalizations.Any(article => article.Locale.Code == locale && article.Status == PublicationStatus.Published && article.ArticleGroup.Authors.Any(author => author.Id == item.Id))).OrderBy(item => item.DisplayName).Select(item => new { item.Slug, title = item.DisplayName }).ToListAsync(token);
        return Results.Ok(new { categories, tags, authors });
    }

    private static async Task<IResult> GetArchiveAsync(string locale, string kind, string slug, PublishingDbContext database, int? page, int? limit, CancellationToken token)
    {
        var take = Math.Clamp(limit ?? 24, 1, 50);
        var currentPage = Math.Max(page ?? 1, 1);
        var query = database.ArticleLocalizations.AsNoTracking().Where(article => article.Locale.Code == locale && article.Locale.IsEnabled && article.Status == PublicationStatus.Published);
        string? title;
        string? description = null;
        object? parent = null;
        object translations = Array.Empty<object>();
        switch (kind)
        {
            case "categories":
                var category = await database.Categories.AsNoTracking().Where(item => item.Locale.Code == locale && item.Slug == slug).Select(item => new { item.Id, item.SourceCategoryId, item.Name, item.Description, Parent = item.ParentCategory == null ? null : new { item.ParentCategory.Slug, title = item.ParentCategory.Name } }).SingleOrDefaultAsync(token);
                if (category is null) return Results.NotFound();
                title = category.Name; description = category.Description; parent = category.Parent;
                query = query.Where(article => article.Categories.Any(item => item.Slug == slug));
                var translationKey = category.SourceCategoryId ?? category.Id;
                translations = await database.Categories.AsNoTracking()
                    .Where(item => (item.Id == translationKey || item.SourceCategoryId == translationKey) && item.Locale.IsEnabled && item.Articles.Any(article => article.Status == PublicationStatus.Published))
                    .OrderBy(item => item.Locale.Code)
                    .Select(item => new { locale = item.Locale.Code, item.Slug })
                    .ToArrayAsync(token);
                break;
            case "tags":
                var tag = await database.Tags.AsNoTracking().Where(item => item.Locale.Code == locale && item.Slug == slug).Select(item => new { item.Id, item.SourceTagId, item.Name }).SingleOrDefaultAsync(token);
                if (tag is null) return Results.NotFound();
                title = tag.Name;
                query = query.Where(article => article.Tags.Any(item => item.Slug == slug));
                var tagTranslationKey = tag.SourceTagId ?? tag.Id;
                translations = await database.Tags.AsNoTracking()
                    .Where(item => (item.Id == tagTranslationKey || item.SourceTagId == tagTranslationKey) && item.Locale.IsEnabled && item.Articles.Any(article => article.Status == PublicationStatus.Published))
                    .OrderBy(item => item.Locale.Code).Select(item => new { locale = item.Locale.Code, item.Slug }).ToArrayAsync(token);
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
        var articleCount = await query.CountAsync(token);
        var totalPages = Math.Max(1, (int)Math.Ceiling(articleCount / (double)take));
        if (currentPage > totalPages) return Results.NotFound();
        var articles = await query.OrderByDescending(article => article.PublishedAt).ThenBy(article => article.Id)
            .Skip((currentPage - 1) * take).Take(take)
            .Select(article => new { article.ArticleGroupId, article.Slug, article.Title, article.Summary, type = article.ArticleGroup.Type.ToString(), article.PublishedAt, article.UpdatedAt,
                sourceCount = article.ArticleGroup.Sources.Count,
                reviewedSourceCount = article.ArticleGroup.Sources.Count(source => source.Kind != SourceKind.Unclassified && source.LastReviewedAt != null),
                cover = article.CoverMediaAssetId == null ? null : new { url = "/api/media/" + article.CoverMediaAssetId + "?v=" + article.CoverMediaAsset!.OptimizedByteLength, altText = article.CoverAltText, article.CoverMediaAsset.FocalX, article.CoverMediaAsset.FocalY } }).ToListAsync(token);
        var typeCounts = await query.GroupBy(article => article.ArticleGroup.Type).Select(group => new { type = group.Key.ToString(), count = group.Count() }).OrderByDescending(item => item.count).ToArrayAsync(token);
        var relatedCategories = await query.SelectMany(article => article.Categories).Where(item => item.Slug != slug)
            .GroupBy(item => new { item.Slug, item.Name }).Select(group => new { group.Key.Slug, title = group.Key.Name, articleCount = group.Count() })
            .OrderByDescending(item => item.articleCount).ThenBy(item => item.title).Take(5).ToArrayAsync(token);
        return Results.Ok(new { kind, slug, title, description, parent, translations, articleCount, page = currentPage, pageSize = take, totalPages, typeCounts, relatedCategories, articles });
    }

    private static async Task<IResult> SearchAsync(string locale, string? q, PublishingDbContext database, int? limit, CancellationToken token)
    {
        var term = PublicSearchQueryPolicy.Normalize(q);
        if (term is null || term.Length < PublicSearchQueryPolicy.MinimumLength) return Results.Ok(Array.Empty<object>());
        if (term.Length > PublicSearchQueryPolicy.MaximumLength) return Results.BadRequest(new { error = $"Search query must not exceed {PublicSearchQueryPolicy.MaximumLength} characters." });
        var escaped = term.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
        var pattern = $"%{escaped}%";
        var take = Math.Clamp(limit ?? 20, 1, 50);
        var articles = await database.ArticleLocalizations.AsNoTracking()
            .Where(article => article.Locale.Code == locale && article.Locale.IsEnabled && article.Status == PublicationStatus.Published)
            .Where(article => EF.Functions.ILike(article.Title, pattern, "\\") || EF.Functions.ILike(article.Summary, pattern, "\\") || EF.Functions.ILike(article.Body, pattern, "\\"))
            .OrderByDescending(article => EF.Functions.ILike(article.Title, escaped, "\\"))
            .ThenByDescending(article => EF.Functions.ILike(article.Title, pattern, "\\"))
            .ThenByDescending(article => EF.Functions.ILike(article.Summary, pattern, "\\"))
            .ThenByDescending(article => article.PublishedAt)
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
                categories = article.Categories.OrderBy(category => category.Name).Select(category => new { category.Slug, category.Name }).Take(2).ToArray(),
                sourceCount = article.ArticleGroup.Sources.Count,
                cover = article.CoverMediaAssetId == null ? null : new { url = "/api/media/" + article.CoverMediaAssetId + "?v=" + article.CoverMediaAsset!.OptimizedByteLength, altText = article.CoverAltText, article.CoverMediaAsset.FocalX, article.CoverMediaAsset.FocalY }
            })
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
                cover = article.CoverMediaAssetId == null ? null : new { url = "/api/media/" + article.CoverMediaAssetId + "?v=" + article.CoverMediaAsset!.OptimizedByteLength, altText = article.CoverAltText, article.CoverMediaAsset.FocalX, article.CoverMediaAsset.FocalY }
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
                cover = item.CoverMediaAssetId == null ? null : new { url = "/api/media/" + item.CoverMediaAssetId + "?v=" + item.CoverMediaAsset!.OptimizedByteLength, altText = item.CoverAltText, caption = item.CoverCaption, credit = item.CoverCredit, item.CoverMediaAsset.FocalX, item.CoverMediaAsset.FocalY },
                type = item.ArticleGroup.Type.ToString(),
                item.PublishedAt,
                item.UpdatedAt,
                categories=item.Categories.Select(x=>new {x.Slug,x.Name}).OrderBy(x=>x.Name).ToArray(),
                tags=item.Tags.Select(x=>new {x.Slug,x.Name}).OrderBy(x=>x.Name).ToArray(),
                authors=item.ArticleGroup.Authors.Select(x=>new {x.Slug,x.DisplayName}).OrderBy(x=>x.DisplayName).ToArray(),
                sources=item.ArticleGroup.Sources.Select(x=>new {x.Name,x.Url}).OrderBy(x=>x.Name).ToArray(),
                corrections = item.Corrections.OrderByDescending(x => x.CorrectedAt).Select(x => new { x.Id, x.Summary, x.Details, x.CorrectedAt }).ToArray(),
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

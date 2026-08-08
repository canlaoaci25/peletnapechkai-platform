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
        return endpoints;
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
            .Select(article => new { article.ArticleGroupId, article.Slug, article.Title, article.Summary, type = article.ArticleGroup.Type.ToString(), article.PublishedAt, article.UpdatedAt })
            .ToListAsync(token);
        return Results.Ok(articles);
    }

    private static async Task<IResult> ListAsync(string locale, PublishingDbContext database, int? limit, CancellationToken token)
    {
        var take = Math.Clamp(limit ?? 12, 1, 50);
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
                article.UpdatedAt
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
                type = item.ArticleGroup.Type.ToString(),
                item.PublishedAt,
                item.UpdatedAt,
                categories=item.Categories.Select(x=>new {x.Slug,x.Name}).OrderBy(x=>x.Name),
                tags=item.Tags.Select(x=>new {x.Slug,x.Name}).OrderBy(x=>x.Name),
                authors=item.ArticleGroup.Authors.Select(x=>new {x.Slug,x.DisplayName}).OrderBy(x=>x.DisplayName),
                sources=item.ArticleGroup.Sources.Select(x=>new {x.Name,x.Url}).OrderBy(x=>x.Name),
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

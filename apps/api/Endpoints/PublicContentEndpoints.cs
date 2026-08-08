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
        group.MapGet("/{locale}/articles/{slug}", GetAsync);
        return endpoints;
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
                type = item.ArticleGroup.Type.ToString(),
                item.PublishedAt,
                item.UpdatedAt
            })
            .SingleOrDefaultAsync(token);
        return article is null ? Results.NotFound() : Results.Ok(article);
    }
}

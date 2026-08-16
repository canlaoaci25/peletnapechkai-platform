using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Domain.Content;
using Peletnapechkai.Api.Infrastructure.Identity;
using Peletnapechkai.Api.Infrastructure.Persistence;

namespace Peletnapechkai.Api.Endpoints;

public static class EditorialCommandCenterEndpoints
{
    public static IEndpointRouteBuilder MapEditorialCommandCenterEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/admin/editorial/command-center", GetAsync)
            .WithTags("Editorial").RequireAuthorization(AuthorizationPolicies.WriteContent);
        return endpoints;
    }

    private static async Task<IResult> GetAsync(PublishingDbContext database, CancellationToken token)
    {
        var now = DateTimeOffset.UtcNow;
        var reviewItems = await database.ArticleLocalizations.AsNoTracking()
            .Where(article => article.Status == PublicationStatus.InEditorialReview || article.Status == PublicationStatus.InSeoReview)
            .OrderBy(article => article.UpdatedAt)
            .Select(article => new EditorialCommandItem(article.Id, article.Title, article.Locale.Code,
                article.Status == PublicationStatus.InEditorialReview ? "EditorialReview" : "SeoReview",
                article.UpdatedAt, null, null, null)).Take(12).ToListAsync(token);
        var taskItems = await database.EditorialTasks.AsNoTracking()
            .Where(task => task.Status != EditorialTaskStatus.Completed).OrderBy(task => task.DueAt)
            .Select(task => new EditorialCommandItem(task.ArticleLocalizationId, task.Article.Title, task.Article.Locale.Code,
                task.DueAt < now ? "OverdueTask" : "Task", task.DueAt, task.Title,
                database.Users.Where(user => user.Id == task.AssigneeUserId).Select(user => user.DisplayName).FirstOrDefault(),
                task.Priority.ToString())).Take(12).ToListAsync(token);
        var incompleteQuality = await database.ArticleQualityChecklists.AsNoTracking()
            .CountAsync(item => !item.TitleAndSummary || !item.SourcesVerified || !item.AuthorAndTaxonomy ||
                !item.SeoMetadata || !item.CoverAccessibility || !item.CommercialDisclosure ||
                !item.TranslationReviewed || !item.LegalEditorialReview, token);
        var items = taskItems.Concat(reviewItems).OrderByDescending(item => EditorialCommandPriority.Score(item.Kind, item.Priority))
            .ThenBy(item => item.DueAt).Take(16).ToArray();
        return Results.Ok(new { checkedAt = now, summary = new {
            overdue = taskItems.Count(item => item.Kind == "OverdueTask"),
            dueSoon = taskItems.Count(item => item.Kind == "Task" && item.DueAt <= now.AddDays(2)),
            inReview = reviewItems.Count, incompleteQuality }, items });
    }
}

public sealed record EditorialCommandItem(Guid ArticleId, string Title, string Locale, string Kind,
    DateTimeOffset DueAt, string? TaskTitle, string? Assignee, string? Priority);

public static class EditorialCommandPriority
{
    public static int Score(string kind, string? priority) => kind switch {
        "OverdueTask" when priority == "Urgent" => 500, "OverdueTask" => 400,
        "Task" when priority == "Urgent" => 300, "EditorialReview" => 220,
        "SeoReview" => 210, "Task" => 100, _ => 0 };
}

using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Domain.Content;
using Peletnapechkai.Api.Infrastructure.Identity;
using Peletnapechkai.Api.Infrastructure.Persistence;

namespace Peletnapechkai.Api.Endpoints;

public static class WebVitalsEndpoints
{
    private static readonly IReadOnlyDictionary<string, double> GoodBudgets = new Dictionary<string, double> { ["LCP"] = 2500, ["CLS"] = .1, ["INP"] = 200 };

    public static IEndpointRouteBuilder MapWebVitalsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/v1/public/web-vitals", RecordAsync).RequireRateLimiting(IdentityServiceExtensions.EngagementRateLimitPolicy).WithTags("Web vitals");
        endpoints.MapGet("/api/v1/admin/web-vitals", DashboardAsync).RequireAuthorization(AuthorizationPolicies.ManageUsers).WithTags("Web vitals");
        return endpoints;
    }

    private static async Task<IResult> RecordAsync(WebVitalRequest request, PublishingDbContext db, TimeProvider clock, CancellationToken token)
    {
        try { db.WebVitalSamples.Add(new WebVitalSample(request.Locale, request.Route, request.Viewport, request.Metric, request.Value, clock.GetUtcNow())); }
        catch (ArgumentException) { return Results.BadRequest(); }
        await db.SaveChangesAsync(token);
        return Results.Accepted();
    }

    private static async Task<IResult> DashboardAsync(PublishingDbContext db, TimeProvider clock, CancellationToken token)
    {
        var since = clock.GetUtcNow().AddDays(-28);
        var samples = await db.WebVitalSamples.AsNoTracking().Where(x => x.MeasuredAt >= since)
            .Select(x => new { x.Locale, x.Route, x.Viewport, x.Metric, x.Value }).ToListAsync(token);
        var cohorts = samples.GroupBy(x => new { x.Locale, x.Route, x.Viewport, x.Metric }).Select(group =>
        {
            var ordered = group.Select(x => x.Value).Order().ToArray();
            var p75 = ordered.Length == 0 ? 0 : ordered[(int)Math.Ceiling(ordered.Length * .75) - 1];
            return new { group.Key.Locale, group.Key.Route, group.Key.Viewport, group.Key.Metric, samples=ordered.Length, p75=Math.Round(p75, group.Key.Metric == "CLS" ? 3 : 0), budget=GoodBudgets[group.Key.Metric], passes=ordered.Length >= 20 && p75 <= GoodBudgets[group.Key.Metric] };
        }).OrderBy(x => x.Locale).ThenBy(x => x.Route).ThenBy(x => x.Viewport).ThenBy(x => x.Metric).ToArray();
        return Results.Ok(new { checkedAt=clock.GetUtcNow(), windowDays=28, minimumSamples=20, privacy="No URL, slug, user or device identifier is stored.", cohorts });
    }

    private sealed record WebVitalRequest(string Locale, string Route, string Viewport, string Metric, double Value);
}

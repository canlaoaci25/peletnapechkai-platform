using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Domain.Automation;
using Peletnapechkai.Api.Infrastructure.Persistence;

namespace Peletnapechkai.Api.Endpoints;

public static class AutomationWorkerEndpoints
{
    public static IEndpointRouteBuilder MapAutomationWorkerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/internal/automation-worker").WithTags("Automation worker");
        group.MapPost("/claim", ClaimAsync);
        group.MapPost("/{id:guid}/complete", CompleteAsync);
        group.MapPost("/{id:guid}/fail", FailAsync);
        group.MapPost("/{id:guid}/retry", RetryAsync);
        return endpoints;
    }

    private static async Task<IResult> ClaimAsync(
        HttpContext context,
        PublishingDbContext database,
        IConfiguration configuration,
        CancellationToken token)
    {
        if (!IsAuthorized(context, configuration)) return Results.Unauthorized();

        var job = await database.AutomationJobs
            .Where(candidate => candidate.Status == AutomationJobStatus.Queued)
            .OrderBy(candidate => candidate.CreatedAt)
            .FirstOrDefaultAsync(token);
        if (job is null) return Results.NoContent();

        job.Start(job.CurrentPhase + 1, DateTimeOffset.UtcNow);
        await database.SaveChangesAsync(token);
        return Results.Ok(new
        {
            job.Id,
            type = job.Type.ToString(),
            job.TargetLocales,
            job.TotalItems,
            job.CurrentPhase,
            prompt = BuildPrompt(job)
        });
    }

    private static async Task<IResult> CompleteAsync(
        Guid id,
        WorkerResult request,
        HttpContext context,
        PublishingDbContext database,
        IConfiguration configuration,
        CancellationToken token)
    {
        if (!IsAuthorized(context, configuration)) return Results.Unauthorized();
        var job = await database.AutomationJobs.SingleOrDefaultAsync(candidate => candidate.Id == id, token);
        if (job is null) return Results.NotFound();
        try
        {
            job.Complete(TrimMessage(request.Message), DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException exception)
        {
            return Results.Conflict(new { message = exception.Message });
        }
        await database.SaveChangesAsync(token);
        return Results.Ok();
    }

    private static async Task<IResult> FailAsync(
        Guid id,
        WorkerResult request,
        HttpContext context,
        PublishingDbContext database,
        IConfiguration configuration,
        CancellationToken token)
    {
        if (!IsAuthorized(context, configuration)) return Results.Unauthorized();
        var job = await database.AutomationJobs.SingleOrDefaultAsync(candidate => candidate.Id == id, token);
        if (job is null) return Results.NotFound();
        try
        {
            job.Fail(TrimMessage(request.Message) ?? "Codex worker işi tamamlayamadı.", DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException exception)
        {
            return Results.Conflict(new { message = exception.Message });
        }
        await database.SaveChangesAsync(token);
        return Results.Ok();
    }

    private static async Task<IResult> RetryAsync(
        Guid id,
        HttpContext context,
        PublishingDbContext database,
        IConfiguration configuration,
        CancellationToken token)
    {
        if (!IsAuthorized(context, configuration)) return Results.Unauthorized();
        var job = await database.AutomationJobs.SingleOrDefaultAsync(candidate => candidate.Id == id, token);
        if (job is null) return Results.NotFound();
        try
        {
            job.Retry(DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException exception)
        {
            return Results.Conflict(new { message = exception.Message });
        }
        await database.SaveChangesAsync(token);
        return Results.Ok();
    }

    private static bool IsAuthorized(HttpContext context, IConfiguration configuration)
    {
        var expected = configuration["Automation:WorkerToken"];
        var supplied = context.Request.Headers["X-BOECL-Worker-Token"].ToString();
        if (string.IsNullOrWhiteSpace(expected) || string.IsNullOrWhiteSpace(supplied)) return false;
        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(supplied));
    }

    private static string BuildPrompt(AutomationJob job)
    {
        var locales = job.TargetLocales.Length == 0 ? "sistem" : string.Join(", ", job.TargetLocales);
        return $"""
            BOECL otomasyon işi {job.Id} üzerinde çalış. İş türü: {job.Type}. Hedef diller: {locales}. Faz: {job.CurrentPhase}.
            Repo AGENTS.md kurallarını eksiksiz uygula. Önce mevcut uygulamayı ve veriyi incele, sonra bu iş türünün eksiklerini kalıcı ve idempotent şekilde tamamla.
            Kullanıcıya görünen yeni içerikler Türkçe olmalıdır; yerelleştirme işinde kaynak Türkçe içeriği değiştirme.
            AI içeriğini veya çeviriyi doğrudan public yayımlama; taslak ya da insan onayı bekleyen durumda bırak.
            Gereken testleri çalıştır. Sır, token veya bağlantı dizesini dosyaya, loga ya da Git'e yazma.
            IIS, Windows hizmetleri ve canlı migration işlemlerini değiştirme. Son mesajda yapılanları, testleri ve kalan riski kısa yaz.
            """;
    }

    private static string? TrimMessage(string? message) =>
        string.IsNullOrWhiteSpace(message) ? null : message.Trim()[..Math.Min(message.Trim().Length, 1900)];

    private sealed record WorkerResult(string? Message);
}

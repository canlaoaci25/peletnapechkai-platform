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
        group.MapPost("/{id:guid}/report", SaveReportAsync);
        return endpoints;
    }

    private static async Task<IResult> ClaimAsync(HttpContext context, PublishingDbContext database, IConfiguration configuration, CancellationToken token)
    {
        if (!IsAuthorized(context, configuration)) return Results.Unauthorized();
        var job = await database.AutomationJobs.Where(candidate => candidate.Status == AutomationJobStatus.Queued).OrderBy(candidate => candidate.CreatedAt).FirstOrDefaultAsync(token);
        if (job is null) return Results.NoContent();
        job.Start(job.CurrentPhase + 1, DateTimeOffset.UtcNow);
        await database.SaveChangesAsync(token);
        return Results.Ok(new { job.Id, type=job.Type.ToString(), job.TargetLocales, job.TotalItems, job.CurrentPhase, prompt=BuildPrompt(job) });
    }

    private static async Task<IResult> CompleteAsync(Guid id, WorkerResult request, HttpContext context, PublishingDbContext database, IConfiguration configuration, CancellationToken token)
    {
        if (!IsAuthorized(context, configuration)) return Results.Unauthorized();
        var job=await database.AutomationJobs.SingleOrDefaultAsync(candidate=>candidate.Id==id,token);if(job is null)return Results.NotFound();
        try { job.Complete(TrimMessage(request.Message),TrimReport(request.Report),DateTimeOffset.UtcNow); }
        catch(InvalidOperationException exception){return Results.Conflict(new{message=exception.Message});}
        await database.SaveChangesAsync(token);return Results.Ok();
    }

    private static async Task<IResult> FailAsync(Guid id, WorkerResult request, HttpContext context, PublishingDbContext database, IConfiguration configuration, CancellationToken token)
    {
        if (!IsAuthorized(context, configuration)) return Results.Unauthorized();
        var job=await database.AutomationJobs.SingleOrDefaultAsync(candidate=>candidate.Id==id,token);if(job is null)return Results.NotFound();
        try { job.Fail(TrimMessage(request.Message)??"Codex worker işi tamamlayamadı.",DateTimeOffset.UtcNow); }
        catch(InvalidOperationException exception){return Results.Conflict(new{message=exception.Message});}
        await database.SaveChangesAsync(token);return Results.Ok();
    }

    private static async Task<IResult> RetryAsync(Guid id,HttpContext context,PublishingDbContext database,IConfiguration configuration,CancellationToken token)
    {
        if(!IsAuthorized(context,configuration))return Results.Unauthorized();var job=await database.AutomationJobs.SingleOrDefaultAsync(candidate=>candidate.Id==id,token);if(job is null)return Results.NotFound();
        try{job.Retry(DateTimeOffset.UtcNow);}catch(InvalidOperationException exception){return Results.Conflict(new{message=exception.Message});}await database.SaveChangesAsync(token);return Results.Ok();
    }

    private static async Task<IResult> SaveReportAsync(Guid id,WorkerResult request,HttpContext context,PublishingDbContext database,IConfiguration configuration,CancellationToken token)
    {
        if(!IsAuthorized(context,configuration))return Results.Unauthorized();var job=await database.AutomationJobs.SingleOrDefaultAsync(candidate=>candidate.Id==id,token);if(job is null)return Results.NotFound();var report=TrimReport(request.Report);if(report is null)return Results.BadRequest(new{message="Rapor metni gereklidir."});job.SetReport(report,DateTimeOffset.UtcNow);await database.SaveChangesAsync(token);return Results.Ok();
    }

    private static bool IsAuthorized(HttpContext context,IConfiguration configuration)
    {
        var expected=configuration["Automation:WorkerToken"];var supplied=context.Request.Headers["X-BOECL-Worker-Token"].ToString();if(string.IsNullOrWhiteSpace(expected)||string.IsNullOrWhiteSpace(supplied))return false;return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected),Encoding.UTF8.GetBytes(supplied));
    }

    private static string BuildPrompt(AutomationJob job)
    {
        var locales=job.TargetLocales.Length==0?"sistem":string.Join(", ",job.TargetLocales);
        var assignment=job.Type switch
        {
            AutomationJobType.SystemReport=>"Yalnız rapor üret; kodu ve veritabanını değiştirme. Git durumunu, son 10 commit'i, mevcut test kanıtlarını, servis durumunu ve dokümantasyonu incele. Bulguları anlaşılır ve ayrıntılı Türkçe raporla; uzun testleri yeniden çalıştırma.",
            AutomationJobType.SiteLocalization=>$"Yalnız apps/web/src/i18n, locale yapılandırması ve doğrudan ilgili bileşenleri incele. Hedef diller ({locales}) için eksik arayüz anahtarlarını kaynak Türkçe sözlüğe göre tamamla. Kaynak Türkçe metni, public içeriği ve veritabanını değiştirme. Sonra lint ve tip denetimini çalıştır.",
            AutomationJobType.ContentTranslation=>$"Hedef diller ({locales}) için yayımlanmış Türkçe içeriklerin eksik çeviri taslaklarını hazırla. Doğrudan public yayın yapma. Güvenli taslak komutu yoksa kodu ve veriyi değiştirmeden engeli ayrıntılı raporla.",
            AutomationJobType.SeoLocalization=>$"Hedef diller ({locales}) için eksik SEO alanlarını insan onayı bekleyen taslak olarak hazırla. Public içeriği değiştirme. Güvenli SEO taslak komutu yoksa kodu ve veriyi değiştirmeden engeli ayrıntılı raporla.",
            _=>throw new InvalidOperationException("Unsupported automation job type.")
        };
        return $"""
            BOECL otomasyon işi {job.Id} üzerinde çalış. İş türü: {job.Type}. Hedef diller: {locales}. Faz: {job.CurrentPhase}.
            {assignment}
            Repo AGENTS.md kurallarını uygula ve incelemeyi belirtilen dizinlerle sınırla. .artifacts, .git, node_modules, bin, obj, IIS yayın klasörleri ve PDF dosyalarını özyinelemeli tarama. PowerShell Get-ChildItem -Recurse kullanma; aramada rg kullan. Kullanıcıya görünen yeni içerikler Türkçe olmalıdır. AI içeriğini veya çeviriyi doğrudan yayımlama. Sırları rapora yazma. IIS, Windows hizmetleri ve canlı migration işlemlerini değiştirme. Son raporu ayrıntılı ve düzgün Türkçe karakterlerle yaz.
            """;
    }

    private static string? TrimMessage(string? message)=>string.IsNullOrWhiteSpace(message)?null:message.Trim()[..Math.Min(message.Trim().Length,1900)];
    private static string? TrimReport(string? report)=>string.IsNullOrWhiteSpace(report)?null:report.Trim()[..Math.Min(report.Trim().Length,100_000)];
    private sealed record WorkerResult(string? Message,string? Report);
}

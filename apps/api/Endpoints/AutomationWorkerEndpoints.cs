using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Domain.Automation;
using Peletnapechkai.Api.Infrastructure.Persistence;
using Peletnapechkai.Api.Infrastructure.Automation;

namespace Peletnapechkai.Api.Endpoints;

public static partial class AutomationWorkerEndpoints
{
    public static IEndpointRouteBuilder MapAutomationWorkerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/internal/automation-worker").WithTags("Automation worker");
        group.MapPost("/claim", ClaimAsync);
        group.MapPost("/{id:guid}/complete", CompleteAsync);
        group.MapPost("/{id:guid}/fail", FailAsync);
        group.MapPost("/{id:guid}/retry", RetryAsync);
        group.MapPost("/{id:guid}/report", SaveReportAsync);
        group.MapGet("/{id:guid}/candidates", GetCandidatesAsync);
        group.MapPost("/{id:guid}/translations", SaveTranslationsAsync);
        group.MapPost("/{id:guid}/category-translations", SaveCategoryTranslationsAsync);
        group.MapPost("/{id:guid}/seo-drafts", SaveSeoDraftsAsync);
        group.MapPost("/{id:guid}/generated-content", SaveGeneratedContentAsync);
        group.MapPost("/{id:guid}/refresh-covers", RefreshGeneratedCoversAsync);
        group.MapPost("/publish-existing-translations", PublishExistingTranslationsAsync);
        return endpoints;
    }

    private static async Task<IResult> ClaimAsync(HttpContext context, PublishingDbContext database, IConfiguration configuration, CancellationToken token)
    {
        if (!IsAuthorized(context, configuration)) return Results.Unauthorized();
        var job = await database.AutomationJobs.Where(candidate => candidate.Status == AutomationJobStatus.Queued).OrderBy(candidate => candidate.CreatedAt).FirstOrDefaultAsync(token);
        if (job is null) return Results.NoContent();
        job.Start(job.CurrentPhase + 1, DateTimeOffset.UtcNow);
        await database.SaveChangesAsync(token);
        return Results.Ok(new { job.Id, type=job.Type.ToString(), job.TargetLocales, job.TotalItems, job.CurrentPhase, job.CategoryId, job.RequestedArticleType, job.IncludeImages, job.AutoTranslate, job.AutoSeo, prompt=BuildPrompt(job) });
    }

    private static async Task<IResult> CompleteAsync(Guid id, WorkerResult request, HttpContext context, PublishingDbContext database, IConfiguration configuration, CancellationToken token)
    {
        if (!IsAuthorized(context, configuration)) return Results.Unauthorized();
        var job=await database.AutomationJobs.SingleOrDefaultAsync(candidate=>candidate.Id==id,token);if(job is null)return Results.NotFound();
        var report = ReadReport(request);
        var remaining = job.Type switch
        {
            AutomationJobType.ContentTranslation => await AutomationCandidateCounter.CountMissingTranslationsAsync(database, job.TargetLocales, token),
            AutomationJobType.SeoLocalization => await AutomationCandidateCounter.CountSeoCandidatesAsync(database, job.TargetLocales, token),
            AutomationJobType.ReadyContentGeneration => await AutomationCandidateCounter.CountReadyContentRemainingAsync(database, job, token),
            AutomationJobType.CategoryLocalization => await AutomationCandidateCounter.CountMissingCategoryTranslationsAsync(database, job.TargetLocales, token),
            _ => 0
        };
        if (!AutomationCompletionPolicy.CanComplete(job.Type, remaining))
        {
            job.SetReport(report, DateTimeOffset.UtcNow);
            job.Fail($"İş çıktısı doğrulanamadı: {remaining} aday hâlâ işlenmeyi bekliyor. İş tamamlanmış sayılmadı.", DateTimeOffset.UtcNow);
            await database.SaveChangesAsync(token);
            return Results.Conflict(new { message = job.LastMessage });
        }
        try { job.Complete(TrimMessage(request.Message),report,DateTimeOffset.UtcNow); }
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
        if(!IsAuthorized(context,configuration))return Results.Unauthorized();var job=await database.AutomationJobs.SingleOrDefaultAsync(candidate=>candidate.Id==id,token);if(job is null)return Results.NotFound();var report=ReadReport(request);if(report is null)return Results.BadRequest(new{message="Rapor metni gereklidir."});job.SetReport(report,DateTimeOffset.UtcNow);await database.SaveChangesAsync(token);return Results.Ok();
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
            AutomationJobType.SystemReport=>"Yalnız rapor üret; kodu ve veritabanını değiştirme. Git durumunu, son 10 commit'i, servis durumunu ve dokümantasyonu incele. Bulunan sorunları ve önerilen düzeltmeleri ayrıntılı Türkçe raporla. Lint, test, build, locale bütünlüğü ve ortam sağlık kapıları worker tarafından ayrıca çalıştırılıp rapora eklenecek; bunları ikinci kez çalıştırma.",
            AutomationJobType.SiteLocalization=>$"Yalnız apps/web/src/i18n, locale yapılandırması ve doğrudan ilgili bileşenleri incele. Hedef diller ({locales}) için eksik arayüz anahtarlarını kaynak Türkçe sözlüğe göre tamamla. Kaynak Türkçe metni, public içeriği ve veritabanını değiştirme. Sonra lint ve tip denetimini çalıştır.",
            AutomationJobType.ContentTranslation=>$"Hedef diller ({locales}) için yalnızca yayımlanmış Türkçe içeriklerin eksik çevirilerini hazırla. Doğrulanmış çeviri teslim API'si yabancı dil makalesini doğrudan yayımlar; ham model çıktısıyla veritabanını değiştirme.",
            AutomationJobType.SeoLocalization=>$"Hedef diller ({locales}) için yayımlanmış çevirilerin eksik SEO alanlarını doğrulanmış SEO teslim API'siyle hazırla. Ham model çıktısıyla veritabanını değiştirme.",
            AutomationJobType.ReadyContentGeneration=>"Seçilen kategori ve türde güncel popüler kaynakları canlı web aramasıyla araştır. Birbirinden ve BOECL arşivinden farklı, ayrıntılı Türkçe makaleler üret. Yalnız yapılandırılmış aday/teslim API'lerini kullan; kaynak URL'lerini eksiksiz bildir.",
            AutomationJobType.CategoryLocalization=>$"Hedef diller ({locales}) için eksik Türkçe kategori adlarını ve slug değerlerini doğal biçimde yerelleştir. Yalnız yapılandırılmış aday/teslim API'sini kullan.",
            _=>throw new InvalidOperationException("Unsupported automation job type.")
        };
        return $"""
            BOECL otomasyon işi {job.Id} üzerinde çalış. İş türü: {job.Type}. Hedef diller: {locales}. Faz: {job.CurrentPhase}.
            {assignment}
            Repo AGENTS.md kurallarını uygula ve incelemeyi belirtilen dizinlerle sınırla. .artifacts, .git, node_modules, bin, obj, IIS yayın klasörleri ve PDF dosyalarını özyinelemeli tarama. PowerShell Get-ChildItem -Recurse kullanma; aramada rg kullan. Manuel yeni içerikler Türkçe olmalıdır; yabancı dil içerikleri yalnızca yayımlanmış Türkçe kaynaktan ve doğrulanan otomasyon teslim API'sinden üretilir. Sırları rapora yazma. IIS, Windows hizmetleri ve canlı migration işlemlerini değiştirme. Son raporu ayrıntılı ve düzgün Türkçe karakterlerle yaz. Her bulguyu Kritik, Hata, Uyarı veya Bilgi önem derecesiyle; kanıt, etki, yapılan işlem ve kalan öneri alanları altında yapılandır. Sorun yoksa bunu açıkça belirt.
            """;
    }

    private static string? TrimMessage(string? message)=>string.IsNullOrWhiteSpace(message)?null:message.Trim()[..Math.Min(message.Trim().Length,1900)];
    private static string? TrimReport(string? report)=>string.IsNullOrWhiteSpace(report)?null:report.Trim()[..Math.Min(report.Trim().Length,100_000)];
    private static string? ReadReport(WorkerResult request)
    {
        if (!string.IsNullOrWhiteSpace(request.ReportBase64))
        {
            try { return TrimReport(Encoding.UTF8.GetString(Convert.FromBase64String(request.ReportBase64))); }
            catch (FormatException) { return null; }
        }
        return TrimReport(request.Report);
    }
    private sealed record WorkerResult(string? Message,string? Report,string? ReportBase64);
}

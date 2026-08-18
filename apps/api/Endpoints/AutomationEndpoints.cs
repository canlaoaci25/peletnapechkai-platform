using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Domain.Automation;
using Peletnapechkai.Api.Domain.Content;
using Peletnapechkai.Api.Domain.Identity;
using Peletnapechkai.Api.Infrastructure.Identity;
using Peletnapechkai.Api.Infrastructure.Automation;
using Peletnapechkai.Api.Infrastructure.Persistence;
using Peletnapechkai.Api.Domain.Auditing;
using System.Text.Json;

namespace Peletnapechkai.Api.Endpoints;

public static class AutomationEndpoints
{
    private static readonly TimeSpan VisualBatchStaleAfter = TimeSpan.FromMinutes(15);
    public static IEndpointRouteBuilder MapAutomationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/admin/automation")
            .RequireAuthorization(AuthorizationPolicies.ManageUsers)
            .WithTags("Automation");

        group.MapGet("/", ListAsync);
        group.MapGet("/{id:guid}", DetailAsync);
        group.MapGet("/scan", ScanAsync);
        group.MapGet("/visual-quality", VisualQualityAsync);
        group.MapPost("/visual-quality/queue", QueueVisualReviewsAsync).ValidateAntiforgery();
        group.MapPost("/visual-quality/batch/{jobId:guid}/{action}", ChangeVisualBatchAsync).ValidateAntiforgery();
        group.MapPost("/visual-quality/{taskId:guid}/{action}", ChangeVisualReviewAsync).ValidateAntiforgery();
        group.MapPost("/", CreateAsync).ValidateAntiforgery();
        group.MapPost("/ready-content", CreateReadyContentAsync).ValidateAntiforgery();
        group.MapGet("/automatic-content", GetAutomaticContentAsync);
        group.MapPut("/automatic-content", UpdateAutomaticContentAsync).ValidateAntiforgery();
        group.MapPost("/{id:guid}/{action}", ChangeStateAsync).ValidateAntiforgery();
        return endpoints;
    }

    private static async Task<IResult> VisualQualityAsync(PublishingDbContext database, IConfiguration configuration, CancellationToken token)
    {
        var rows = await database.ArticleLocalizations.AsNoTracking()
            .Where(article => article.Status == PublicationStatus.Published)
            .OrderByDescending(article => article.PublishedAt)
            .Select(article => new
            {
                article.Id, locale = article.Locale.Code, article.Slug, article.Title, article.Summary, article.Body,
                article.CoverAltText, article.CoverCredit, article.PublishedAt,
                coverId = article.CoverMediaAssetId, width = article.CoverMediaAsset == null ? null : article.CoverMediaAsset.Width,
                height = article.CoverMediaAsset == null ? null : article.CoverMediaAsset.Height,
                optimizedBytes = article.CoverMediaAsset == null ? null : article.CoverMediaAsset.OptimizedByteLength,
                categories = article.Categories.Select(category => category.Name).ToArray()
            }).ToListAsync(token);
        var taskRows = await database.VisualReviewTasks.AsNoTracking().ToListAsync(token);
        var activeBatch = await database.AutomationJobs.AsNoTracking()
            .Where(job => job.Type == AutomationJobType.VisualRenewal && job.Status != AutomationJobStatus.Cancelled)
            .OrderByDescending(job => job.CreatedAt)
            .Select(job => new { job.Id, status = job.Status.ToString(), job.TotalItems, job.CompletedItems, job.FailedItems,
                job.CurrentPhase, job.LastMessage, job.UpdatedAt }).FirstOrDefaultAsync(token);
        var tasks = taskRows.GroupBy(task => task.ArticleLocalizationId).ToDictionary(group => group.Key, group => group.OrderByDescending(task => task.CreatedAt).First());
        var items = rows.Select(row =>
        {
            var result = ArticleVisualQualityPolicy.Assess(new(row.Title, row.Summary, row.Body, row.CoverAltText,
                row.CoverCredit, row.width, row.height, row.optimizedBytes, row.coverId is not null));
            return new { row.Id, row.locale, row.Slug, row.Title, row.PublishedAt, score = result.Score, grade = result.Grade,
                risks = result.Risks, result.BodyImageCount, coverUrl = row.coverId is null ? null : "/api/media/" + row.coverId,
                row.CoverAltText, row.width, row.height, row.optimizedBytes,
                sectionPlan = VisualBriefBuilder.BuildSectionPlan(row.Title, row.Summary, row.Body, row.locale, row.categories),
                visualTask = tasks.TryGetValue(row.Id, out var task) ? new { task.Id, status = task.Status.ToString(), target = task.Target.ToString(), task.SectionHeading, task.SectionContext, task.VisualPurpose, visualType = InferVisualType(task.ProposedPrompt), task.ProposedPrompt, task.NegativePrompt, task.AttemptCount, task.ReviewerNote, task.UpdatedAt,
                    task.CandidateMediaAssetId, candidateUrl = task.CandidateMediaAssetId == null ? null : "/api/media/" + task.CandidateMediaAssetId,
                    task.Provider, task.LicenseName, task.Attribution, task.CandidateAltText, task.TopicScore, task.TextSafetyScore, task.CropScore, task.OriginalityScore,
                    candidateEvidenceVersion = task.CandidateMediaAssetId == null ? null : "editorial-attestation-v2", candidateAttestedAt = task.CandidateMediaAssetId == null ? null : task.ReviewedAt,
                    task.ClosestMediaAssetId, task.ClosestSimilarityPercent, closestMediaUrl = task.ClosestMediaAssetId == null ? null : "/api/media/" + task.ClosestMediaAssetId,
                    task.CandidatePasses, task.PromotedAt, task.LeaseOwner, task.LeaseExpiresAt, task.NextAttemptAt,
                    task.LastFailureCode, task.DeadLetteredAt } : null };
        }).OrderBy(item => item.score).ThenByDescending(item => item.PublishedAt).ToArray();
        return Results.Ok(new
        {
            checkedAt = DateTimeOffset.UtcNow, total = items.Length, passing = items.Count(item => item.score >= 80 && item.risks.Length == 0),
            needsReview = items.Count(item => item.risks.Length > 0), missingCover = items.Count(item => item.risks.Contains("missing-cover")),
            textRisk = items.Count(item => item.risks.Contains("text-risk")), averageScore = items.Length == 0 ? 0 : Math.Round(items.Average(item => item.score), 1),
            queued = taskRows.Count(task => task.Status is VisualReviewStatus.Pending or VisualReviewStatus.InReview or VisualReviewStatus.RetryRequested),
            approved = taskRows.Count(task => task.Status == VisualReviewStatus.Approved),
            rejected = taskRows.Count(task => task.Status == VisualReviewStatus.Rejected),
            deadLetter = taskRows.Count(task => task.Status == VisualReviewStatus.DeadLetter),
            leased = taskRows.Count(task => task.LeaseToken != null && task.LeaseExpiresAt > DateTimeOffset.UtcNow),
            deferred = taskRows.Count(task => task.NextAttemptAt > DateTimeOffset.UtcNow),
            providers = VisualProviderHealthPolicy.Assess(configuration),
            batch = activeBatch == null ? null : new { activeBatch.Id, activeBatch.status, activeBatch.TotalItems,
                processed = taskRows.Count(task => task.AutomationJobId == activeBatch.Id && task.Status is VisualReviewStatus.Approved or VisualReviewStatus.Rejected),
                remaining = taskRows.Count(task => task.AutomationJobId == activeBatch.Id && task.Status is VisualReviewStatus.Pending or VisualReviewStatus.InReview or VisualReviewStatus.RetryRequested),
                successful = taskRows.Count(task => task.AutomationJobId == activeBatch.Id && task.Status == VisualReviewStatus.Approved),
                rejected = taskRows.Count(task => task.AutomationJobId == activeBatch.Id && task.Status == VisualReviewStatus.Rejected),
                activeArticle = items.FirstOrDefault(item => item.visualTask != null && taskRows.Any(task => task.Id == item.visualTask.Id && task.AutomationJobId == activeBatch.Id && task.Status is VisualReviewStatus.Pending or VisualReviewStatus.InReview or VisualReviewStatus.RetryRequested))?.Title,
                activeBatch.CurrentPhase, activeBatch.LastMessage, activeBatch.UpdatedAt,
                isStale = activeBatch.status == nameof(AutomationJobStatus.Running) && activeBatch.UpdatedAt <= DateTimeOffset.UtcNow.Subtract(VisualBatchStaleAfter),
                staleAfterMinutes = (int)VisualBatchStaleAfter.TotalMinutes }, items
        });
    }

    private static async Task<IResult> QueueVisualReviewsAsync(System.Security.Claims.ClaimsPrincipal principal,
        UserManager<ApplicationUser> users, PublishingDbContext database, CancellationToken token)
    {
        var actor = await users.GetUserAsync(principal); if (actor is null) return Results.Unauthorized();
        var existingBatch = await database.AutomationJobs.AnyAsync(job => job.Type == AutomationJobType.VisualRenewal &&
            job.Status != AutomationJobStatus.Completed && job.Status != AutomationJobStatus.Cancelled && job.Status != AutomationJobStatus.Failed, token);
        if (existingBatch) return Results.Conflict(new { message = "An active visual renewal batch already exists." });
        var rows = await database.ArticleLocalizations
            .Where(article => article.Status == PublicationStatus.Published)
            .Select(article => new { article.Id, article.Title, article.Summary, article.Body, locale = article.Locale.Code,
                article.CoverAltText, article.CoverCredit, coverId = article.CoverMediaAssetId,
                width = article.CoverMediaAsset == null ? null : article.CoverMediaAsset.Width,
                height = article.CoverMediaAsset == null ? null : article.CoverMediaAsset.Height,
                optimizedBytes = article.CoverMediaAsset == null ? null : article.CoverMediaAsset.OptimizedByteLength,
                categories = article.Categories.Select(category => category.Name).ToArray() }).ToListAsync(token);
        var keys = await database.VisualReviewTasks.Select(task => task.IdempotencyKey).ToHashSetAsync(token);
        var assessments = rows.Select(row => new { row, quality = ArticleVisualQualityPolicy.Assess(new(row.Title, row.Summary, row.Body, row.CoverAltText, row.CoverCredit, row.width, row.height, row.optimizedBytes, row.coverId is not null)) })
            .Where(item => item.quality.Score < 80 || item.quality.Risks.Length > 0).ToArray();
        var candidates = assessments.Select(item =>
        {
            var section = item.quality.Risks.Contains("missing-body-visual")
                ? VisualBriefBuilder.BuildSectionPlan(item.row.Title, item.row.Summary, item.row.Body, item.row.locale, item.row.categories).FirstOrDefault()
                : null;
            var target = section is null ? VisualReviewTarget.Cover : VisualReviewTarget.BodySection;
            var key = target == VisualReviewTarget.Cover
                ? $"cover:{item.row.Id}:{string.Join('-', item.quality.Risks.Order())}"
                : $"body:{item.row.Id}:{section!.Heading.ToLowerInvariant()}";
            return new { item.row, item.quality, section, target, key };
        })
            .Where(item => !keys.Contains(item.key)).ToArray();
        if (candidates.Length == 0) return Results.Ok(new { id = (Guid?)null, created = 0, skipped = rows.Count, total = 0 });
        var now = DateTimeOffset.UtcNow; var created = 0;
        var batch = new AutomationJob(AutomationJobType.VisualRenewal, candidates.Select(item => item.row.locale), candidates.Length, actor.Id, now);
        database.AutomationJobs.Add(batch);
        foreach (var item in candidates)
        {
            var row = item.row; var quality = item.quality;
            var brief = item.section is null
                ? VisualBriefBuilder.Build(row.Title, row.Summary, row.Body, row.locale, row.categories)
                : new VisualBrief(item.section.Heading, item.section.Purpose, item.section.VisualType, item.section.TypeReason, item.section.Prompt, item.section.NegativePrompt);
            database.VisualReviewTasks.Add(new(row.Id, item.target == VisualReviewTarget.Cover ? row.coverId : null, quality.Score,
                string.Join(',', quality.Risks), brief.SectionContext, brief.Purpose, brief.Prompt, brief.NegativePrompt,
                item.key, now, batch.Id, item.target, item.section?.Heading));
            created++;
        }
        database.AuditLogs.Add(new AuditLog(actor.Id, "visual-renewal.batch_created", nameof(AutomationJob), batch.Id,
            JsonSerializer.Serialize(new { total = candidates.Length, created, locales = batch.TargetLocales }), now));
        await database.SaveChangesAsync(token);
        return Results.Ok(new { batch.Id, created, skipped = rows.Count - created, total = candidates.Length });
    }

    private static async Task<IResult> ChangeVisualBatchAsync(Guid jobId, string action,
        System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users,
        PublishingDbContext database, CancellationToken token)
    {
        var actor = await users.GetUserAsync(principal); if (actor is null) return Results.Unauthorized();
        var job = await database.AutomationJobs.SingleOrDefaultAsync(candidate => candidate.Id == jobId && candidate.Type == AutomationJobType.VisualRenewal, token);
        if (job is null) return Results.NotFound();
        var now = DateTimeOffset.UtcNow;
        var previousStatus = job.Status;
        var previousUpdatedAt = job.UpdatedAt;
        try
        {
            switch (action.ToLowerInvariant())
            {
                case "start": job.Start(Math.Max(1, job.CurrentPhase), now); break;
                case "pause": job.Pause(now); break;
                case "resume": job.Resume(now); break;
                case "cancel": job.Cancel(now); break;
                case "recover":
                    if (job.Status != AutomationJobStatus.Running || job.UpdatedAt > now.Subtract(VisualBatchStaleAfter))
                        return Results.Conflict(new { message = "Only a stale running visual batch can be recovered." });
                    job.RecoverStaleRun(now);
                    break;
                default: return Results.ValidationProblem(new Dictionary<string, string[]> { ["action"] = ["Unsupported batch action."] });
            }
        }
        catch (InvalidOperationException error) { return Results.Conflict(new { message = error.Message }); }
        database.AuditLogs.Add(new AuditLog(actor.Id, $"visual-renewal.batch_{action.ToLowerInvariant()}", nameof(AutomationJob), job.Id,
            JsonSerializer.Serialize(new { previousStatus = previousStatus.ToString(), status = job.Status.ToString(), previousUpdatedAt,
                staleThresholdMinutes = (int)VisualBatchStaleAfter.TotalMinutes, job.CompletedItems, job.FailedItems, job.CurrentPhase }), now));
        try { await database.SaveChangesAsync(token); }
        catch (DbUpdateConcurrencyException)
        {
            return Results.Conflict(new { message = "The visual batch changed while this action was being applied. Refresh before retrying." });
        }
        return Results.Ok(new { job.Id, status = job.Status.ToString(), job.CurrentPhase, job.UpdatedAt });
    }

    private static async Task<IResult> ChangeVisualReviewAsync(Guid taskId, string action, VisualReviewActionRequest request,
        System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext database,
        IConfiguration configuration, CancellationToken token)
    {
        var actor = await users.GetUserAsync(principal); if (actor is null) return Results.Unauthorized();
        var task = await database.VisualReviewTasks.SingleOrDefaultAsync(candidate => candidate.Id == taskId, token);
        if (task is null) return Results.NotFound();
        if (action.Equals("candidate", StringComparison.OrdinalIgnoreCase))
        {
            if (request.MediaAssetId is null) return Results.ValidationProblem(new Dictionary<string, string[]> { ["mediaAssetId"] = ["A candidate media asset is required."] });
            var media = await database.MediaAssets.SingleOrDefaultAsync(x => x.Id == request.MediaAssetId, token);
            if (media is null) return Results.NotFound();
            if (media.Width is null || media.Height is null || media.OptimizedStorageKey is null || media.OptimizedByteLength > 500_000 || media.Width < 1200 || Math.Abs(media.Width.Value / (double)media.Height.Value - 16d / 9d) > .12)
                return Results.ValidationProblem(new Dictionary<string, string[]> { ["mediaAssetId"] = ["Candidate must be optimized, at least 1200px wide, under 500KB, and approximately 16:9."] });
            VisualSimilarityResult similarity;
            try
            {
                var mediaRoot = Path.GetFullPath(configuration["Media:StoragePath"] ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "BOECL", "Media"));
                var candidatePath = ResolveMediaPath(mediaRoot, media.OptimizedStorageKey);
                var candidateHash = VisualSimilarityAnalyzer.ComputeDifferenceHash(candidatePath);
                media.SetPerceptualHash(candidateHash);
                var archive = await database.MediaAssets.Where(x => x.Id != media.Id && x.OptimizedStorageKey != null).ToListAsync(token);
                foreach (var asset in archive.Where(x => x.PerceptualHash == null))
                {
                    try { asset.SetPerceptualHash(VisualSimilarityAnalyzer.ComputeDifferenceHash(ResolveMediaPath(mediaRoot, asset.OptimizedStorageKey))); }
                    catch (Exception error) when (error is IOException or InvalidDataException) { }
                }
                similarity = VisualSimilarityAnalyzer.Assess(candidateHash, archive.Where(x => x.PerceptualHash != null).Select(x => (x.Id, x.PerceptualHash!)));
            }
            catch (Exception error) when (error is IOException or InvalidDataException)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]> { ["mediaAssetId"] = ["Candidate similarity analysis failed. Verify the optimized media asset and retry."] });
            }
            var ratio = media.Width!.Value / (double)media.Height!.Value;
            var cropScore = Math.Abs(ratio - 16d / 9d) <= .04 ? 100 : 80;
            try { task.AttachCandidate(media.Id, request.Provider ?? "", request.LicenseName ?? "", request.Attribution, request.AltText ?? "",
                request.ArticleConfirmed, request.SectionConfirmed, request.LocaleConfirmed, request.TechnicalAccuracyConfirmed,
                request.TextAndLogoFreeConfirmed, request.ArtifactFreeConfirmed, request.CropConfirmed, actor.Id, cropScore,
                similarity.OriginalityScore, similarity.ClosestMediaAssetId, similarity.ClosestSimilarityPercent, DateTimeOffset.UtcNow); }
            catch (ArgumentException error) { return Results.ValidationProblem(new Dictionary<string, string[]> { ["candidate"] = [error.Message] }); }
            await ReconcileVisualBatchAsync(task, database, DateTimeOffset.UtcNow, token);
            database.AuditLogs.Add(new AuditLog(actor.Id, "visual-review.candidate_attached", nameof(VisualReviewTask), task.Id,
                JsonSerializer.Serialize(new { media.Id, request.Provider, request.LicenseName, evidence = "editorial-attestation-v2",
                    request.ArticleConfirmed, request.SectionConfirmed, request.LocaleConfirmed, request.TechnicalAccuracyConfirmed,
                    request.TextAndLogoFreeConfirmed, request.ArtifactFreeConfirmed, request.CropConfirmed,
                    candidateAttestedAt = task.ReviewedAt, task.TopicScore, task.TextSafetyScore, task.CropScore,
                    similarity.OriginalityScore, similarity.ClosestMediaAssetId, similarity.ClosestSimilarityPercent }), DateTimeOffset.UtcNow));
            try { await database.SaveChangesAsync(token); }
            catch (DbUpdateConcurrencyException) { return Results.Conflict(new { message = "The visual review changed. Refresh before retrying." }); }
            return Results.Ok(new { task.Id, task.CandidatePasses, task.UpdatedAt });
        }
        if (action.Equals("promote", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(request.Note)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["note"] = ["Promotion requires an editorial note."] });
            if (!task.CandidatePasses || task.CandidateMediaAssetId is null) return Results.Conflict(new { message = "Candidate has not passed every quality gate." });
            var article = await database.ArticleLocalizations.SingleOrDefaultAsync(x => x.Id == task.ArticleLocalizationId, token);
            var candidate = await database.MediaAssets.SingleOrDefaultAsync(x => x.Id == task.CandidateMediaAssetId, token);
            if (article is null || candidate is null) return Results.NotFound();
            await using var transaction = await database.Database.BeginTransactionAsync(token); var now = DateTimeOffset.UtcNow;
            if (task.Target == VisualReviewTarget.BodySection)
                article.PromoteReviewedBodyImage(candidate, task.SectionHeading!, task.CandidateAltText!, task.Attribution ?? task.LicenseName!, now);
            else
                article.PromoteReviewedCover(candidate, task.CandidateAltText!, task.Attribution ?? task.LicenseName!, now);
            task.MarkPromoted(actor.Id, request.Note, now);
            await ReconcileVisualBatchAsync(task, database, now, token);
            database.AuditLogs.Add(new AuditLog(actor.Id, "visual-review.promoted", nameof(VisualReviewTask), task.Id,
                JsonSerializer.Serialize(new { task.ArticleLocalizationId, task.Target, task.SectionHeading, previousMediaAssetId = task.CurrentMediaAssetId, candidateMediaAssetId = candidate.Id, task.Provider, task.LicenseName }), now));
            try { await database.SaveChangesAsync(token); }
            catch (DbUpdateConcurrencyException) { return Results.Conflict(new { message = "The visual review changed. Refresh before retrying." }); }
            await transaction.CommitAsync(token);
            return Results.Ok(new { task.Id, status = task.Status.ToString(), task.PromotedAt });
        }
        var status = action.ToLowerInvariant() switch { "review" => VisualReviewStatus.InReview,
            "reject" => VisualReviewStatus.Rejected, "retry" => VisualReviewStatus.RetryRequested, _ => (VisualReviewStatus?)null };
        if (status is null) return Results.ValidationProblem(new Dictionary<string, string[]> { ["action"] = ["Unsupported visual review action."] });
        if (status is VisualReviewStatus.Approved or VisualReviewStatus.Rejected && string.IsNullOrWhiteSpace(request.Note))
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["note"] = ["Editorial decisions require a note."] });
        var invalidatedCandidateId = task.CandidateMediaAssetId;
        var invalidatedEvidence = task.CandidateMediaAssetId == null ? null : new { version = "editorial-attestation-v2", task.TopicScore, task.TextSafetyScore, task.CropScore, task.OriginalityScore };
        task.ChangeStatus(status.Value, actor.Id, request.Note, DateTimeOffset.UtcNow);
        await ReconcileVisualBatchAsync(task, database, DateTimeOffset.UtcNow, token);
        database.AuditLogs.Add(new AuditLog(actor.Id, $"visual-review.{action.ToLowerInvariant()}", nameof(VisualReviewTask), task.Id,
            JsonSerializer.Serialize(new { task.ArticleLocalizationId, status = status.ToString(), note = request.Note, invalidatedCandidateId, invalidatedEvidence }), DateTimeOffset.UtcNow));
        try { await database.SaveChangesAsync(token); }
        catch (DbUpdateConcurrencyException) { return Results.Conflict(new { message = "The visual review changed. Refresh before retrying." }); }
        return Results.Ok(new { task.Id, status = task.Status.ToString(), task.AttemptCount, task.UpdatedAt });
    }

    private static async Task ReconcileVisualBatchAsync(VisualReviewTask task, PublishingDbContext database,
        DateTimeOffset now, CancellationToken token)
    {
        if (task.AutomationJobId is not Guid jobId) return;
        var job = await database.AutomationJobs.SingleOrDefaultAsync(candidate => candidate.Id == jobId, token);
        if (job is null || job.Status == AutomationJobStatus.Cancelled) return;
        var tasks = await database.VisualReviewTasks.Where(candidate => candidate.AutomationJobId == jobId).ToListAsync(token);
        var successful = tasks.Count(candidate => candidate.Status == VisualReviewStatus.Approved);
        var rejected = tasks.Count(candidate => candidate.Status == VisualReviewStatus.Rejected);
        job.ReconcileVisualCheckpoint(successful, rejected, tasks.Count - successful - rejected, now);
    }

    private sealed record VisualReviewActionRequest(string? Note, Guid? MediaAssetId = null, string? Provider = null,
        string? LicenseName = null, string? Attribution = null, string? AltText = null,
        bool ArticleConfirmed = false, bool SectionConfirmed = false, bool LocaleConfirmed = false,
        bool TechnicalAccuracyConfirmed = false, bool TextAndLogoFreeConfirmed = false,
        bool ArtifactFreeConfirmed = false, bool CropConfirmed = false);

    private static string InferVisualType(string prompt)
    {
        foreach (var type in new[] { "step-by-step editorial illustration", "comparison editorial illustration", "data-led editorial illustration", "technical editorial illustration", "natural editorial photograph" })
            if (prompt.Contains(type, StringComparison.OrdinalIgnoreCase)) return type;
        return "editorial visual";
    }

    private static string ResolveMediaPath(string root, string? storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey)) throw new InvalidDataException("Optimized media path is missing.");
        var path = Path.GetFullPath(Path.Combine(root, storageKey.Replace('/', Path.DirectorySeparatorChar)));
        if (!path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Media path is outside the configured storage root.");
        if (!File.Exists(path)) throw new FileNotFoundException("Optimized media file is missing.", path);
        return path;
    }

    private static async Task<IResult> ListAsync(PublishingDbContext database, CancellationToken token) =>
        Results.Ok(await database.AutomationJobs
            .AsNoTracking()
            .OrderByDescending(job => job.CreatedAt)
            .Take(100)
            .Select(job => new
            {
                job.Id,
                type = job.Type.ToString(),
                status = job.Status.ToString(),
                job.TargetLocales,
                job.TotalItems,
                job.CompletedItems,
                job.FailedItems,
                job.CurrentPhase,
                job.LastMessage,
                job.CreatedAt,
                job.UpdatedAt,
                job.CompletedAt
                ,job.CategoryId, job.RequestedArticleType, job.IncludeImages, job.AutoTranslate, job.AutoSeo,
                job.IsAutomaticallyScheduled,
                categoryName = database.Categories.Where(category => category.Id == job.CategoryId).Select(category => category.Name).FirstOrDefault(),
                turkishPublished = database.ArticleLocalizations.Count(article => article.GeneratedByAutomationJobId == job.Id && article.Locale.IsDefault && article.Status == PublicationStatus.Published),
                translationPublished = database.ArticleLocalizations.Count(article => article.GeneratedByAutomationJobId == job.Id && !article.Locale.IsDefault && article.Status == PublicationStatus.Published),
                seoComplete = database.ArticleLocalizations.Count(article => article.GeneratedByAutomationJobId == job.Id && article.Status == PublicationStatus.Published && article.SeoTitle != null && article.SeoDescription != null),
                latestContentAt = database.ArticleLocalizations.Where(article => article.GeneratedByAutomationJobId == job.Id).Max(article => (DateTimeOffset?)article.CreatedAt),
                recentArticles = database.ArticleLocalizations.Where(article => article.GeneratedByAutomationJobId == job.Id && article.Locale.IsDefault).OrderByDescending(article => article.CreatedAt).Take(3).Select(article => new { article.Title, article.Slug, locale = article.Locale.Code }).ToArray()
            })
            .ToListAsync(token));

    private static async Task<IResult> DetailAsync(Guid id, PublishingDbContext database, CancellationToken token)
    {
        var job = await database.AutomationJobs.AsNoTracking()
            .Where(candidate => candidate.Id == id)
            .Select(candidate => new
            {
                candidate.Id,
                type = candidate.Type.ToString(),
                status = candidate.Status.ToString(),
                candidate.TargetLocales,
                candidate.TotalItems,
                candidate.CompletedItems,
                candidate.FailedItems,
                candidate.CurrentPhase,
                candidate.LastMessage,
                candidate.ReportText,
                candidate.CreatedAt,
                candidate.UpdatedAt,
                candidate.CompletedAt
                ,candidate.CategoryId, candidate.RequestedArticleType, candidate.IncludeImages, candidate.AutoTranslate, candidate.AutoSeo
            })
            .SingleOrDefaultAsync(token);
        return job is null ? Results.NotFound() : Results.Ok(job);
    }

    private static async Task<IResult> ScanAsync(
        PublishingDbContext database,
        IConfiguration configuration,
        CancellationToken token)
    {
        var activeLocales = await database.Locales
            .AsNoTracking()
            .Where(locale => locale.IsEnabled)
            .OrderByDescending(locale => locale.IsDefault)
            .ThenBy(locale => locale.Code)
            .Select(locale => locale.Code)
            .ToArrayAsync(token);

        var publishedArticles = await database.ArticleLocalizations
            .AsNoTracking()
            .CountAsync(article => article.Status == PublicationStatus.Published, token);
        var defaultLocale = await database.Locales.AsNoTracking().Where(locale => locale.IsDefault).Select(locale => locale.Code).SingleAsync(token);
        var targetLocales = activeLocales.Where(locale => !locale.Equals(defaultLocale, StringComparison.OrdinalIgnoreCase)).ToArray();
        var missingTranslations = await AutomationCandidateCounter.CountMissingTranslationsAsync(database, targetLocales, token);
        var seoCandidates = await AutomationCandidateCounter.CountSeoCandidatesAsync(database, targetLocales, token);
        var categoryCandidates = await AutomationCandidateCounter.CountMissingCategoryTranslationsAsync(database, targetLocales, token);
        var seoTargetLocales = await AutomationCandidateCounter.GetSeoCandidateLocalesAsync(database, targetLocales, token);
        var completedSiteLocaleSets = await database.AutomationJobs
            .AsNoTracking()
            .Where(job => job.Type == AutomationJobType.SiteLocalization && job.Status == AutomationJobStatus.Completed)
            .Select(job => job.TargetLocales)
            .ToListAsync(token);
        var completedSiteLocales = completedSiteLocaleSets.SelectMany(locales => locales)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var pendingSiteLocales = activeLocales.Count(locale => locale != "tr-TR" && !completedSiteLocales.Contains(locale));

        return Results.Ok(new
        {
            activeLocales,
            publishedArticles,
            missingTranslations,
            seoCandidates,
            siteLanguageCandidates = pendingSiteLocales,
            reportCandidates = 0,
            runnerEnabled = configuration.GetValue<bool>("Automation:RunnerEnabled"),
            workloads = new
            {
                contentTranslation = new { count = missingTranslations, targetLocales, blockedReason = missingTranslations == 0 ? "Çevrilecek eksik içerik bulunmuyor." : null },
                categoryLocalization = new { count = categoryCandidates, targetLocales, blockedReason = categoryCandidates == 0 ? "Eksik kategori çevirisi bulunmuyor." : null },
                seoLocalization = new { count = seoCandidates, targetLocales = seoTargetLocales, blockedReason = seoCandidates == 0 && missingTranslations > 0 ? "Önce hedef dillerde içerik çeviri taslakları oluşturulmalıdır." : seoCandidates == 0 ? "SEO yerelleştirmesi bekleyen hedef dil kaydı bulunmuyor." : null },
                siteLocalization = new { count = pendingSiteLocales, targetLocales = targetLocales.Where(locale => !completedSiteLocales.Contains(locale)).ToArray(), blockedReason = pendingSiteLocales == 0 ? "Eksik site dili bulunmuyor." : null },
                systemReport = new { count = 0, targetLocales = Array.Empty<string>(), blockedReason = (string?)null }
            }
        });
    }

    private static async Task<IResult> CreateAsync(
        CreateRequest request,
        System.Security.Claims.ClaimsPrincipal principal,
        UserManager<ApplicationUser> users,
        PublishingDbContext database,
        IConfiguration configuration,
        CancellationToken token)
    {
        if (!configuration.GetValue<bool>("Automation:RunnerEnabled"))
        {
            return Results.Conflict(new { message = "Codex worker etkinleştirilmeden toplu iş başlatılamaz." });
        }

        if (!Enum.TryParse<AutomationJobType>(request.Type, true, out var type))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["type"] = ["Desteklenmeyen toplu iş türü."]
            });
        }
        if (type == AutomationJobType.ReadyContentGeneration)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["type"] = ["Hazır içerik işleri özel oluşturma ekranından başlatılmalıdır."] });

        var actor = await users.GetUserAsync(principal);
        if (actor is null)
        {
            return Results.Unauthorized();
        }

        var activeLocales = await database.Locales
            .AsNoTracking()
            .Where(locale => locale.IsEnabled)
            .Select(locale => locale.Code)
            .ToArrayAsync(token);
        var requestedLocales = request.TargetLocales
            .Where(activeLocales.Contains)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (type is not AutomationJobType.SystemReport && requestedLocales.Length == 0)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["targetLocales"] = ["En az bir etkin hedef dil gereklidir."]
            });
        }

        var duplicateExists = await database.AutomationJobs.AnyAsync(job =>
            job.Type == type &&
            (job.Status == AutomationJobStatus.Queued ||
             job.Status == AutomationJobStatus.Running ||
             job.Status == AutomationJobStatus.Paused), token);
        if (duplicateExists)
        {
            return Results.Conflict(new { message = "Bu tür için zaten etkin bir toplu iş var." });
        }

        var totalItems = await CountItemsAsync(type, requestedLocales, database, token);
        if (totalItems == 0)
        {
            var message = type == AutomationJobType.SeoLocalization
                ? "SEO yerelleştirmesi için önce hedef dillerde içerik çeviri taslakları oluşturulmalıdır."
                : "Bu iş türü için işlenecek eksik kayıt bulunamadı.";
            return Results.Conflict(new { message });
        }

        var job = new AutomationJob(type, requestedLocales, totalItems, actor.Id, DateTimeOffset.UtcNow);
        database.AutomationJobs.Add(job);
        await database.SaveChangesAsync(token);
        return Results.Created($"/api/v1/admin/automation/{job.Id}", new { job.Id });
    }

    private static async Task<IResult> GetAutomaticContentAsync(PublishingDbContext database, CancellationToken token)
    {
        var schedule = await database.AutomaticContentSchedules.AsNoTracking().SingleOrDefaultAsync(token);
        return Results.Ok(new { isEnabled = schedule?.IsEnabled ?? false, intervalMinutes = schedule?.IntervalMinutes ?? 3,
            schedule?.NextRunAt, schedule?.LastEnqueuedAt, schedule?.LastJobId });
    }

    private static async Task<IResult> UpdateAutomaticContentAsync(AutomaticContentRequest request,
        System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users,
        PublishingDbContext database, IConfiguration configuration, CancellationToken token)
    {
        if (request.IsEnabled && !configuration.GetValue<bool>("Automation:RunnerEnabled"))
            return Results.Conflict(new { message = "Codex worker etkin değil." });
        var actor = await users.GetUserAsync(principal); if (actor is null) return Results.Unauthorized();
        var now = DateTimeOffset.UtcNow;
        var schedule = await database.AutomaticContentSchedules.SingleOrDefaultAsync(token);
        if (schedule is null) { schedule = new AutomaticContentSchedule(actor.Id, now); database.AutomaticContentSchedules.Add(schedule); }
        schedule.SetEnabled(request.IsEnabled, actor.Id, now);
        await database.SaveChangesAsync(token);
        return Results.Ok(new { schedule.IsEnabled, schedule.IntervalMinutes, schedule.NextRunAt, schedule.LastEnqueuedAt, schedule.LastJobId });
    }

    private static async Task<IResult> CreateReadyContentAsync(
        ReadyContentRequest request,
        System.Security.Claims.ClaimsPrincipal principal,
        UserManager<ApplicationUser> users,
        PublishingDbContext database,
        IConfiguration configuration,
        CancellationToken token)
    {
        if (!configuration.GetValue<bool>("Automation:RunnerEnabled")) return Results.Conflict(new { message = "Codex worker etkin değil." });
        if (request.Count is < 1 or > 50 || !Enum.TryParse<ArticleType>(request.ArticleType, true, out var articleType))
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["request"] = ["Geçerli tür ve 1-50 arasında adet gereklidir."] });
        var actor = await users.GetUserAsync(principal); if (actor is null) return Results.Unauthorized();
        var category = await database.Categories.AsNoTracking().Include(item => item.Locale)
            .SingleOrDefaultAsync(item => item.Id == request.CategoryId && item.Locale.IsDefault, token);
        if (category is null) return Results.ValidationProblem(new Dictionary<string, string[]> { ["categoryId"] = ["Geçerli bir Türkçe kategori gereklidir."] });
        var duplicate = await database.AutomationJobs.AnyAsync(job => job.Type == AutomationJobType.ReadyContentGeneration &&
            (job.Status == AutomationJobStatus.Queued || job.Status == AutomationJobStatus.Running || job.Status == AutomationJobStatus.Paused), token);
        if (duplicate) return Results.Conflict(new { message = "Zaten etkin bir hazır içerik üretim işi var." });
        var targetLocales = request.AutoTranslate
            ? await database.Locales.AsNoTracking().Where(locale => locale.IsEnabled && !locale.IsDefault).Select(locale => locale.Code).Order().ToArrayAsync(token)
            : [];
        var job = new AutomationJob(AutomationJobType.ReadyContentGeneration, targetLocales, request.Count, actor.Id, DateTimeOffset.UtcNow);
        job.ConfigureContentGeneration(category.Id, articleType.ToString(), request.IncludeImages, request.AutoTranslate, request.AutoSeo);
        database.AutomationJobs.Add(job); await database.SaveChangesAsync(token);
        return Results.Created($"/api/v1/admin/automation/{job.Id}", new { job.Id });
    }

    private static async Task<int> CountItemsAsync(
        AutomationJobType type,
        string[] targetLocales,
        PublishingDbContext database,
        CancellationToken token)
    {
        if (type == AutomationJobType.SystemReport)
        {
            return 1;
        }

        if (type == AutomationJobType.SiteLocalization)
        {
            return targetLocales.Length;
        }

        if (type == AutomationJobType.SeoLocalization)
        {
            return await AutomationCandidateCounter.CountSeoCandidatesAsync(database, targetLocales, token);
        }

        if (type == AutomationJobType.CategoryLocalization)
        {
            return await AutomationCandidateCounter.CountMissingCategoryTranslationsAsync(database, targetLocales, token);
        }

        return await AutomationCandidateCounter.CountMissingTranslationsAsync(database, targetLocales, token);
    }

    private static async Task<IResult> ChangeStateAsync(
        Guid id,
        string action,
        PublishingDbContext database,
        CancellationToken token)
    {
        var job = await database.AutomationJobs.SingleOrDefaultAsync(candidate => candidate.Id == id, token);
        if (job is null)
        {
            return Results.NotFound();
        }

        try
        {
            var now = DateTimeOffset.UtcNow;
            switch (action.ToLowerInvariant())
            {
                case "pause": job.Pause(now); break;
                case "resume": job.Resume(now); break;
                case "retry": job.Retry(now); break;
                case "cancel": job.Cancel(now); break;
                default: return Results.NotFound();
            }
        }
        catch (InvalidOperationException exception)
        {
            return Results.Conflict(new { message = exception.Message });
        }

        await database.SaveChangesAsync(token);
        return Results.Ok(new { job.Id, status = job.Status.ToString() });
    }

    private sealed record CreateRequest(string Type, string[] TargetLocales);
    private sealed record ReadyContentRequest(Guid CategoryId, string ArticleType, int Count, bool IncludeImages, bool AutoTranslate, bool AutoSeo);
    private sealed record AutomaticContentRequest(bool IsEnabled);
}

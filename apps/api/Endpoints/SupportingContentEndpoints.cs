using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Domain.Auditing;
using Peletnapechkai.Api.Domain.Content;
using Peletnapechkai.Api.Domain.Identity;
using Peletnapechkai.Api.Infrastructure.Identity;
using Peletnapechkai.Api.Infrastructure.Persistence;
using SkiaSharp;

namespace Peletnapechkai.Api.Endpoints;

public static partial class SupportingContentEndpoints
{
    private const long MaxUploadBytes = 10 * 1024 * 1024;

    public static IEndpointRouteBuilder MapSupportingContentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var supporting = endpoints.MapGroup("/api/v1/admin/supporting").WithTags("Supporting content")
            .RequireAuthorization(AuthorizationPolicies.ManageEditorial);
        supporting.MapGet("/", ListAsync);
        supporting.MapPost("/categories", CreateCategoryAsync).ValidateAntiforgery();
        supporting.MapPut("/categories/{categoryId:guid}", UpdateCategoryAsync).ValidateAntiforgery();
        supporting.MapDelete("/categories/{categoryId:guid}", DeleteCategoryAsync).ValidateAntiforgery();
        supporting.MapPost("/tags", CreateTagAsync).ValidateAntiforgery();
        supporting.MapPut("/tags/{tagId:guid}", UpdateTagAsync).ValidateAntiforgery();
        supporting.MapDelete("/tags/{tagId:guid}", DeleteTagAsync).ValidateAntiforgery();
        supporting.MapPost("/authors", CreateAuthorAsync).ValidateAntiforgery();
        supporting.MapPost("/sources", CreateSourceAsync).ValidateAntiforgery();
        supporting.MapPut("/sources/{sourceId:guid}/review", ReviewSourceAsync).ValidateAntiforgery();

        var media = endpoints.MapGroup("/api/v1/admin/media").WithTags("Media")
            .RequireAuthorization(AuthorizationPolicies.ManageEditorial);
        media.MapGet("/", ListMediaAsync);
        media.MapGet("/{assetId:guid}", GetAdminMediaAsync);
        media.MapPost("/", UploadMediaAsync).ValidateAntiforgery();
        media.MapPost("/{assetId:guid}/delete-unused", DeleteUnusedMediaAsync).ValidateAntiforgery();
        return endpoints;
    }

    private static async Task<IResult> ListAsync(PublishingDbContext db, CancellationToken token) => Results.Ok(new
    {
        categories = await db.Categories.AsNoTracking().OrderBy(x => x.Locale.Code).ThenBy(x => x.Name).Select(x => new
        {
            x.Id, locale = x.Locale.Code, x.Slug, x.Name,
            x.ParentCategoryId, parentName = x.ParentCategory == null ? null : x.ParentCategory.Name,
            childCount = x.Children.Count,
            articleCount = x.Articles.Count,
            publishedCount = x.Articles.Count(article => article.Status == PublicationStatus.Published)
        }).ToListAsync(token),
        tags = await db.Tags.AsNoTracking().OrderBy(x => x.Locale.Code).ThenBy(x => x.Name).Select(x => new { x.Id, locale = x.Locale.Code, x.Slug, x.Name }).ToListAsync(token),
        authors = await db.Authors.AsNoTracking().OrderBy(x => x.DisplayName).Select(x => new { x.Id, x.Slug, x.DisplayName }).ToListAsync(token),
        sources = await db.Sources.AsNoTracking().OrderBy(x => x.Name).Select(x => new { x.Id, x.Name, x.Url, kind=x.Kind.ToString(), x.LastReviewedAt }).ToListAsync(token),
        taxonomyHealth = new
        {
            publishedCount = db.ArticleLocalizations.Count(article => article.Locale.Code == "tr-TR" && article.Status == PublicationStatus.Published),
            uncategorizedCount = db.ArticleLocalizations.Count(article => article.Locale.Code == "tr-TR" && article.Status == PublicationStatus.Published && !article.Categories.Any()),
            uncategorized = db.ArticleLocalizations.AsNoTracking()
                .Where(article => article.Locale.Code == "tr-TR" && article.Status == PublicationStatus.Published && !article.Categories.Any())
                .OrderByDescending(article => article.PublishedAt).Take(12)
                .Select(article => new { article.Id, article.Slug, article.Title, article.PublishedAt }).ToArray()
        }
    });

    private static async Task<IResult> CreateCategoryAsync(NamedLocaleRequest request, System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext db, CancellationToken token)
    {
        if(request.Locale!="tr-TR")return Invalid();
        var locale = await db.Locales.SingleOrDefaultAsync(x => x.Code == request.Locale && x.IsEnabled, token);
        if (locale is null || !ValidSlug(request.Slug) || string.IsNullOrWhiteSpace(request.Name)) return Invalid();
        var item = new Category(locale, request.Slug, request.Name, DateTimeOffset.UtcNow);
        return await AddAsync(item, "supporting.category_created", principal, users, db, token);
    }

    private static async Task<IResult> UpdateCategoryAsync(Guid categoryId,NamedRequest request,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser> users,PublishingDbContext db,CancellationToken token)
    {
        if(!ValidSlug(request.Slug)||string.IsNullOrWhiteSpace(request.Name))return Invalid();
        var actor=await users.GetUserAsync(principal);var item=await db.Categories.Include(x=>x.Locale).SingleOrDefaultAsync(x=>x.Id==categoryId,token);
        if(actor is null)return Results.Unauthorized();if(item is null)return Results.NotFound();if(item.Locale.Code!="tr-TR")return Results.Conflict(new{message="Only Turkish categories are managed here."});
        Category? parent=null;
        if(request.ParentCategoryId.HasValue){parent=await db.Categories.SingleOrDefaultAsync(x=>x.Id==request.ParentCategoryId&&x.LocaleId==item.LocaleId,token);if(parent is null||parent.ParentCategoryId!=null||parent.Id==item.Id)return Results.ValidationProblem(new Dictionary<string,string[]>{{"parentCategoryId",["A valid top-level category is required."]}});}
        item.Update(request.Slug,request.Name);item.SetParent(parent);db.AuditLogs.Add(new AuditLog(actor.Id,"supporting.category_updated",nameof(Category),item.Id,JsonSerializer.Serialize(new{item.Slug,item.Name,item.ParentCategoryId}),DateTimeOffset.UtcNow));
        try{await db.SaveChangesAsync(token);}catch(DbUpdateException){return Results.Conflict(new{message="A category with the same slug already exists."});}
        return Results.Ok(new{item.Id,item.Slug,item.Name});
    }

    private static async Task<IResult> DeleteCategoryAsync(Guid categoryId,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser> users,PublishingDbContext db,CancellationToken token)
    {
        var actor=await users.GetUserAsync(principal);var item=await db.Categories.Include(x=>x.Locale).Include(x=>x.Articles).SingleOrDefaultAsync(x=>x.Id==categoryId,token);
        if(actor is null)return Results.Unauthorized();if(item is null)return Results.NotFound();if(item.Locale.Code!="tr-TR")return Results.Conflict(new{message="Only Turkish categories are managed here."});
        if(item.Articles.Count>0||await db.Categories.AnyAsync(x=>x.ParentCategoryId==item.Id,token))return Results.Conflict(new{message="A category used by content or child topics cannot be deleted."});
        db.AuditLogs.Add(new AuditLog(actor.Id,"supporting.category_deleted",nameof(Category),item.Id,JsonSerializer.Serialize(new{item.Slug,item.Name}),DateTimeOffset.UtcNow));db.Categories.Remove(item);await db.SaveChangesAsync(token);return Results.NoContent();
    }

    private static async Task<IResult> CreateTagAsync(NamedLocaleRequest request, System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext db, CancellationToken token)
    {
        if(request.Locale!="tr-TR")return Invalid();
        var locale = await db.Locales.SingleOrDefaultAsync(x => x.Code == request.Locale && x.IsEnabled, token);
        if (locale is null || !ValidSlug(request.Slug) || string.IsNullOrWhiteSpace(request.Name)) return Invalid();
        var item = new Tag(locale, request.Slug, request.Name, DateTimeOffset.UtcNow);
        return await AddAsync(item, "supporting.tag_created", principal, users, db, token);
    }

    private static async Task<IResult> UpdateTagAsync(Guid tagId,NamedRequest request,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser> users,PublishingDbContext db,CancellationToken token)
    {
        if(!ValidSlug(request.Slug)||string.IsNullOrWhiteSpace(request.Name))return Invalid();
        var actor=await users.GetUserAsync(principal);var item=await db.Tags.Include(x=>x.Locale).SingleOrDefaultAsync(x=>x.Id==tagId,token);
        if(actor is null)return Results.Unauthorized();if(item is null)return Results.NotFound();if(item.Locale.Code!="tr-TR")return Results.Conflict(new{message="Only Turkish tags are managed here."});
        item.Update(request.Slug,request.Name);db.AuditLogs.Add(new AuditLog(actor.Id,"supporting.tag_updated",nameof(Tag),item.Id,null,DateTimeOffset.UtcNow));
        try{await db.SaveChangesAsync(token);}catch(DbUpdateException){return Results.Conflict(new{message="A tag with the same slug already exists."});}
        return Results.Ok(new{item.Id,item.Slug,item.Name});
    }

    private static async Task<IResult> DeleteTagAsync(Guid tagId,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser> users,PublishingDbContext db,CancellationToken token)
    {
        var actor=await users.GetUserAsync(principal);var item=await db.Tags.Include(x=>x.Locale).Include(x=>x.Articles).SingleOrDefaultAsync(x=>x.Id==tagId,token);
        if(actor is null)return Results.Unauthorized();if(item is null)return Results.NotFound();if(item.Locale.Code!="tr-TR")return Results.Conflict(new{message="Only Turkish tags are managed here."});
        if(item.Articles.Count>0)return Results.Conflict(new{message="A tag used by content cannot be deleted."});
        db.AuditLogs.Add(new AuditLog(actor.Id,"supporting.tag_deleted",nameof(Tag),item.Id,JsonSerializer.Serialize(new{item.Slug,item.Name}),DateTimeOffset.UtcNow));db.Tags.Remove(item);await db.SaveChangesAsync(token);return Results.NoContent();
    }

    private static Task<IResult> CreateAuthorAsync(NamedRequest request, System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext db, CancellationToken token) =>
        !ValidSlug(request.Slug) || string.IsNullOrWhiteSpace(request.Name) ? Task.FromResult(Invalid()) : AddAsync(new Author(request.Slug, request.Name, DateTimeOffset.UtcNow), "supporting.author_created", principal, users, db, token);

    private static Task<IResult> CreateSourceAsync(SourceRequest request, System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext db, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 200 ||
            !Uri.TryCreate(request.Url, UriKind.Absolute, out var url) || !Source.TryNormalizePublicUrl(url, out _)) return Task.FromResult(Invalid());
        return AddAsync(new Source(request.Name, url, DateTimeOffset.UtcNow), "supporting.source_created", principal, users, db, token);
    }

    private static async Task<IResult> ReviewSourceAsync(Guid sourceId, SourceReviewRequest request, System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext db, CancellationToken token)
    {
        var actor = await users.GetUserAsync(principal); if (actor is null) return Results.Unauthorized();
        if (!Enum.TryParse<SourceKind>(request.Kind, true, out var kind) || kind == SourceKind.Unclassified) return Invalid();
        var source = await db.Sources.SingleOrDefaultAsync(x => x.Id == sourceId, token); if (source is null) return Results.NotFound();
        source.Review(kind, DateTimeOffset.UtcNow);
        db.AuditLogs.Add(new AuditLog(actor.Id, "supporting.source_reviewed", nameof(Source), source.Id, JsonSerializer.Serialize(new { kind = kind.ToString() }), DateTimeOffset.UtcNow));
        await db.SaveChangesAsync(token); return Results.Ok(new { source.Id, kind = source.Kind.ToString(), source.LastReviewedAt });
    }

    private static async Task<IResult> AddAsync<T>(T item, string action, System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext db, CancellationToken token) where T : class
    {
        var actor = await users.GetUserAsync(principal);
        if (actor is null) return Results.Unauthorized();
        db.Add(item);
        var id = (Guid)(typeof(T).GetProperty("Id")?.GetValue(item) ?? Guid.Empty);
        db.AuditLogs.Add(new AuditLog(actor.Id, action, typeof(T).Name, id, null, DateTimeOffset.UtcNow));
        try { await db.SaveChangesAsync(token); }
        catch (DbUpdateException) { return Results.Conflict(new { message = "Aynı anahtara sahip bir kayıt zaten var." }); }
        return Results.Created($"/api/v1/admin/supporting/{id}", new { id });
    }

    private static async Task<IResult> ListMediaAsync(PublishingDbContext db, CancellationToken token) => Results.Ok(await db.MediaAssets.AsNoTracking().OrderByDescending(x => x.CreatedAt).Take(200).Select(x => new { x.Id, x.FileName, x.ContentType, x.ByteLength, x.Width, x.Height, x.OptimizedByteLength, x.CreatedAt, usageCount=db.ArticleGroups.Count(group=>group.MediaAssets.Any(media=>media.Id==x.Id))+db.ArticleLocalizations.Count(article=>article.CoverMediaAssetId==x.Id), canDelete=x.CreatedAt<DateTimeOffset.UtcNow.AddHours(-24)&&!db.ArticleGroups.Any(group=>group.MediaAssets.Any(media=>media.Id==x.Id))&&!db.ArticleLocalizations.Any(article=>article.CoverMediaAssetId==x.Id) }).ToListAsync(token));

    private static async Task<IResult> GetAdminMediaAsync(Guid assetId, PublishingDbContext db, IConfiguration config, CancellationToken token)
    {
        var asset = await db.MediaAssets.AsNoTracking().Where(item => item.Id == assetId).Select(item => new { item.StorageKey, item.ContentType, item.CreatedAt }).SingleOrDefaultAsync(token);
        if (asset is null) return Results.NotFound();
        var root = Path.GetFullPath(config["Media:StoragePath"] ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "BOECL", "Media"));
        var path = Path.GetFullPath(Path.Combine(root, asset.StorageKey.Replace('/', Path.DirectorySeparatorChar)));
        if (!path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || !File.Exists(path)) return Results.NotFound();
        return Results.File(path, asset.ContentType, lastModified: asset.CreatedAt, enableRangeProcessing: true);
    }

    private static async Task<IResult> UploadMediaAsync(HttpRequest request, IConfiguration config, System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext db, CancellationToken token)
    {
        if (!request.HasFormContentType) return Results.BadRequest(new { message = "Multipart form bekleniyor." });
        var form = await request.ReadFormAsync(token);
        var file = form.Files.GetFile("file");
        if (file is null || file.Length is <= 0 or > MaxUploadBytes) return Results.BadRequest(new { message = "Dosya 1 bayt ile 10 MB arasında olmalıdır." });
        await using var input = file.OpenReadStream();
        var header = new byte[12];
        var read = await input.ReadAsync(header, token);
        if (!MediaUploadValidator.TryValidate(file.ContentType, header.AsSpan(0, read), out var extension)) return Results.BadRequest(new { message = "Yalnızca gerçek JPEG, PNG veya WebP görselleri kabul edilir." });
        input.Position = 0;

        var root = Path.GetFullPath(config["Media:StoragePath"] ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "BOECL", "Media"));
        var storageKey = Path.Combine(DateTime.UtcNow.ToString("yyyy"), DateTime.UtcNow.ToString("MM"), $"{Guid.CreateVersion7()}{extension}");
        var destination = Path.GetFullPath(Path.Combine(root, storageKey));
        if (!destination.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) return Results.BadRequest();
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        await using (var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true)) await input.CopyToAsync(output, token);

        var optimizedKey=Path.Combine(DateTime.UtcNow.ToString("yyyy"),DateTime.UtcNow.ToString("MM"),$"{Guid.CreateVersion7()}-cover.webp");
        var optimizedPath=Path.GetFullPath(Path.Combine(root,optimizedKey));
        int width;int height;long optimizedLength;
        try
        {
            using var codec=SKCodec.Create(destination);var info=codec?.Info??throw new InvalidDataException("Image cannot be decoded.");
            if(info.Width<=0||info.Height<=0||info.Width>20000||info.Height>20000||(long)info.Width*info.Height>40_000_000)throw new InvalidDataException("Image dimensions are not allowed.");
            using var original=SKBitmap.Decode(destination)??throw new InvalidDataException("Image cannot be decoded.");width=original.Width;height=original.Height;
            var targetWidth=Math.Min(1200,original.Width);var targetHeight=Math.Max(1,(int)Math.Round(original.Height*(targetWidth/(double)original.Width)));
            using var optimized=targetWidth==original.Width?original.Copy():original.Resize(new SKImageInfo(targetWidth,targetHeight),new SKSamplingOptions(SKFilterMode.Linear,SKMipmapMode.Linear));
            if(optimized is null)throw new InvalidDataException("Image cannot be resized.");using var optimizedImage=SKImage.FromBitmap(optimized);using var encoded=optimizedImage.Encode(SKEncodedImageFormat.Webp,82);await using var optimizedOutput=new FileStream(optimizedPath,FileMode.CreateNew,FileAccess.Write,FileShare.None,81920,true);encoded.SaveTo(optimizedOutput);await optimizedOutput.FlushAsync(token);optimizedLength=optimizedOutput.Length;
        }
        catch
        {
            File.Delete(destination);File.Delete(optimizedPath);return Results.BadRequest(new{message="Görsel güvenli biçimde işlenemedi veya boyut sınırını aşıyor."});
        }

        var actor = await users.GetUserAsync(principal);
        if (actor is null) { File.Delete(destination); return Results.Unauthorized(); }
        var safeName = Path.GetFileName(file.FileName);
        var asset = new MediaAsset(storageKey.Replace('\\', '/'), safeName, file.ContentType.ToLowerInvariant(), file.Length, DateTimeOffset.UtcNow);
        asset.SetImageMetadata(width,height,optimizedKey.Replace('\\','/'),optimizedLength);
        db.MediaAssets.Add(asset);
        db.AuditLogs.Add(new AuditLog(actor.Id, "media.uploaded", nameof(MediaAsset), asset.Id, JsonSerializer.Serialize(new { asset.FileName, asset.ByteLength }), DateTimeOffset.UtcNow));
        try { await db.SaveChangesAsync(token); }
        catch { File.Delete(destination);File.Delete(optimizedPath);throw; }
        return Results.Created($"/api/v1/admin/media/{asset.Id}", new { asset.Id, asset.FileName, asset.ContentType, asset.ByteLength, asset.Width, asset.Height, asset.OptimizedByteLength, asset.CreatedAt });
    }

    private static async Task<IResult> DeleteUnusedMediaAsync(Guid assetId,IConfiguration config,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser> users,PublishingDbContext db,CancellationToken token)
    {
        var actor=await users.GetUserAsync(principal);if(actor is null)return Results.Unauthorized();
        var asset=await db.MediaAssets.SingleOrDefaultAsync(x=>x.Id==assetId,token);if(asset is null)return Results.NotFound();
        var used=await db.ArticleGroups.AnyAsync(group=>group.MediaAssets.Any(media=>media.Id==assetId),token)||await db.ArticleLocalizations.AnyAsync(article=>article.CoverMediaAssetId==assetId,token);
        if(used||asset.CreatedAt>=DateTimeOffset.UtcNow.AddHours(-24))return Results.Conflict(new{message="Only media unused for at least 24 hours can be deleted."});
        var root=Path.GetFullPath(config["Media:StoragePath"]??Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),"BOECL","Media"));
        var paths=new[]{asset.StorageKey,asset.OptimizedStorageKey}.Where(key=>!string.IsNullOrWhiteSpace(key)).Select(key=>Path.GetFullPath(Path.Combine(root,key!.Replace('/',Path.DirectorySeparatorChar)))).Where(path=>path.StartsWith(root+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase)).ToArray();
        db.AuditLogs.Add(new AuditLog(actor.Id,"media.deleted_unused",nameof(MediaAsset),asset.Id,JsonSerializer.Serialize(new{asset.FileName,asset.ByteLength,asset.OptimizedByteLength}),DateTimeOffset.UtcNow));db.MediaAssets.Remove(asset);await db.SaveChangesAsync(token);
        foreach(var path in paths)File.Delete(path);return Results.NoContent();
    }

    private static bool ValidSlug(string? value) => value is { Length: <= 160 } && SlugPattern().IsMatch(value);
    private static IResult Invalid() => Results.ValidationProblem(new Dictionary<string, string[]> { ["request"] = ["Geçerli ve eksiksiz alanlar gereklidir."] });
    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugPattern();
    private sealed record NamedLocaleRequest(string Locale, string Slug, string Name);
    private sealed record NamedRequest(string Slug, string Name, Guid? ParentCategoryId = null);
    private sealed record SourceRequest(string Name, string Url);
    private sealed record SourceReviewRequest(string Kind);
}

public static class MediaUploadValidator
{
    public static bool TryValidate(string contentType, ReadOnlySpan<byte> bytes, out string extension)
    {
        extension = string.Empty;
        if (contentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase) && bytes.Length >= 3 && bytes[0] == 0xff && bytes[1] == 0xd8 && bytes[2] == 0xff) extension = ".jpg";
        else if (contentType.Equals("image/png", StringComparison.OrdinalIgnoreCase) && bytes.Length >= 8 && bytes[..8].SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })) extension = ".png";
        else if (contentType.Equals("image/webp", StringComparison.OrdinalIgnoreCase) && bytes.Length >= 12 && bytes[..4].SequenceEqual("RIFF"u8) && bytes.Slice(8, 4).SequenceEqual("WEBP"u8)) extension = ".webp";
        return extension.Length > 0;
    }
}

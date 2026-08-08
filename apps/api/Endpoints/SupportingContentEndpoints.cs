using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Domain.Auditing;
using Peletnapechkai.Api.Domain.Content;
using Peletnapechkai.Api.Domain.Identity;
using Peletnapechkai.Api.Infrastructure.Identity;
using Peletnapechkai.Api.Infrastructure.Persistence;

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
        supporting.MapPost("/tags", CreateTagAsync).ValidateAntiforgery();
        supporting.MapPost("/authors", CreateAuthorAsync).ValidateAntiforgery();
        supporting.MapPost("/sources", CreateSourceAsync).ValidateAntiforgery();

        var media = endpoints.MapGroup("/api/v1/admin/media").WithTags("Media")
            .RequireAuthorization(AuthorizationPolicies.ManageEditorial);
        media.MapGet("/", ListMediaAsync);
        media.MapPost("/", UploadMediaAsync).ValidateAntiforgery();
        return endpoints;
    }

    private static async Task<IResult> ListAsync(PublishingDbContext db, CancellationToken token) => Results.Ok(new
    {
        categories = await db.Categories.AsNoTracking().OrderBy(x => x.Locale.Code).ThenBy(x => x.Name).Select(x => new { x.Id, locale = x.Locale.Code, x.Slug, x.Name }).ToListAsync(token),
        tags = await db.Tags.AsNoTracking().OrderBy(x => x.Locale.Code).ThenBy(x => x.Name).Select(x => new { x.Id, locale = x.Locale.Code, x.Slug, x.Name }).ToListAsync(token),
        authors = await db.Authors.AsNoTracking().OrderBy(x => x.DisplayName).Select(x => new { x.Id, x.Slug, x.DisplayName }).ToListAsync(token),
        sources = await db.Sources.AsNoTracking().OrderBy(x => x.Name).Select(x => new { x.Id, x.Name, x.Url }).ToListAsync(token)
    });

    private static async Task<IResult> CreateCategoryAsync(NamedLocaleRequest request, System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext db, CancellationToken token)
    {
        var locale = await db.Locales.SingleOrDefaultAsync(x => x.Code == request.Locale && x.IsEnabled, token);
        if (locale is null || !ValidSlug(request.Slug) || string.IsNullOrWhiteSpace(request.Name)) return Invalid();
        var item = new Category(locale, request.Slug, request.Name, DateTimeOffset.UtcNow);
        return await AddAsync(item, "supporting.category_created", principal, users, db, token);
    }

    private static async Task<IResult> CreateTagAsync(NamedLocaleRequest request, System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext db, CancellationToken token)
    {
        var locale = await db.Locales.SingleOrDefaultAsync(x => x.Code == request.Locale && x.IsEnabled, token);
        if (locale is null || !ValidSlug(request.Slug) || string.IsNullOrWhiteSpace(request.Name)) return Invalid();
        var item = new Tag(locale, request.Slug, request.Name, DateTimeOffset.UtcNow);
        return await AddAsync(item, "supporting.tag_created", principal, users, db, token);
    }

    private static Task<IResult> CreateAuthorAsync(NamedRequest request, System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext db, CancellationToken token) =>
        !ValidSlug(request.Slug) || string.IsNullOrWhiteSpace(request.Name) ? Task.FromResult(Invalid()) : AddAsync(new Author(request.Slug, request.Name, DateTimeOffset.UtcNow), "supporting.author_created", principal, users, db, token);

    private static Task<IResult> CreateSourceAsync(SourceRequest request, System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext db, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || !Uri.TryCreate(request.Url, UriKind.Absolute, out var url) || url.Scheme is not ("http" or "https")) return Task.FromResult(Invalid());
        return AddAsync(new Source(request.Name, url, DateTimeOffset.UtcNow), "supporting.source_created", principal, users, db, token);
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

    private static async Task<IResult> ListMediaAsync(PublishingDbContext db, CancellationToken token) => Results.Ok(await db.MediaAssets.AsNoTracking().OrderByDescending(x => x.CreatedAt).Take(200).Select(x => new { x.Id, x.FileName, x.ContentType, x.ByteLength, x.CreatedAt }).ToListAsync(token));

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

        var actor = await users.GetUserAsync(principal);
        if (actor is null) { File.Delete(destination); return Results.Unauthorized(); }
        var safeName = Path.GetFileName(file.FileName);
        var asset = new MediaAsset(storageKey.Replace('\\', '/'), safeName, file.ContentType.ToLowerInvariant(), file.Length, DateTimeOffset.UtcNow);
        db.MediaAssets.Add(asset);
        db.AuditLogs.Add(new AuditLog(actor.Id, "media.uploaded", nameof(MediaAsset), asset.Id, JsonSerializer.Serialize(new { asset.FileName, asset.ByteLength }), DateTimeOffset.UtcNow));
        try { await db.SaveChangesAsync(token); }
        catch { File.Delete(destination); throw; }
        return Results.Created($"/api/v1/admin/media/{asset.Id}", new { asset.Id, asset.FileName, asset.ContentType, asset.ByteLength, asset.CreatedAt });
    }

    private static bool ValidSlug(string? value) => value is { Length: <= 160 } && SlugPattern().IsMatch(value);
    private static IResult Invalid() => Results.ValidationProblem(new Dictionary<string, string[]> { ["request"] = ["Geçerli ve eksiksiz alanlar gereklidir."] });
    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugPattern();
    private sealed record NamedLocaleRequest(string Locale, string Slug, string Name);
    private sealed record NamedRequest(string Slug, string Name);
    private sealed record SourceRequest(string Name, string Url);
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

using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Infrastructure.Identity;
using Peletnapechkai.Api.Infrastructure.Persistence;

namespace Peletnapechkai.Api.Endpoints;
public static class SystemStatusEndpoints
{
    public static IEndpointRouteBuilder MapSystemStatusEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/admin/status", async (PublishingDbContext db, IConfiguration config, CancellationToken token) =>
        {
            var mediaRoot=Path.GetFullPath(config["Media:StoragePath"]??Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),"BOECL","Media"));long mediaBytes=0;int mediaFiles=0;if(Directory.Exists(mediaRoot))foreach(var file in Directory.EnumerateFiles(mediaRoot,"*",SearchOption.AllDirectories)){var info=new FileInfo(file);mediaBytes+=info.Length;mediaFiles++;}var drive=new DriveInfo(Path.GetPathRoot(mediaRoot)!);
            return Results.Ok(new {checkedAt=DateTimeOffset.UtcNow,database="healthy",articles=await db.ArticleLocalizations.CountAsync(token),published=await db.ArticleLocalizations.CountAsync(x=>x.Status==Domain.Content.PublicationStatus.Published,token),users=await db.Users.CountAsync(token),mediaFiles,mediaBytes,diskFreeBytes=drive.AvailableFreeSpace});
        }).RequireAuthorization(AuthorizationPolicies.ManageUsers).WithTags("Operations");return endpoints;
    }
}

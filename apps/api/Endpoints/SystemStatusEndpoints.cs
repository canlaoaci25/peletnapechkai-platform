using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Infrastructure.Identity;
using Peletnapechkai.Api.Infrastructure.Persistence;
using Peletnapechkai.Api.Infrastructure.Operations;

namespace Peletnapechkai.Api.Endpoints;
public static class SystemStatusEndpoints
{
    public static IEndpointRouteBuilder MapSystemStatusEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/admin/status", async (PublishingDbContext db, IConfiguration config, ProductionHealthSnapshotReader healthReader, DeploymentSnapshotReader deploymentReader, CancellationToken token) =>
        {
            var mediaRoot=Path.GetFullPath(config["Media:StoragePath"]??Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),"BOECL","Media"));long mediaBytes=0;int mediaFiles=0;if(Directory.Exists(mediaRoot))foreach(var file in Directory.EnumerateFiles(mediaRoot,"*",SearchOption.AllDirectories)){var info=new FileInfo(file);mediaBytes+=info.Length;mediaFiles++;}var drive=new DriveInfo(Path.GetPathRoot(mediaRoot)!);
            var lifecycle=await db.ArticleLocalizations.AsNoTracking().GroupBy(x=>x.Status).Select(group=>new{status=group.Key.ToString(),count=group.Count()}).ToDictionaryAsync(x=>x.status,x=>x.count,token);
            var types=await db.ArticleLocalizations.AsNoTracking().GroupBy(x=>x.ArticleGroup.Type).Select(group=>new{type=group.Key.ToString(),count=group.Count()}).ToDictionaryAsync(x=>x.type,x=>x.count,token);
            var articles=lifecycle.Values.Sum();var published=lifecycle.GetValueOrDefault(Domain.Content.PublicationStatus.Published.ToString());
            var deploymentHistory=deploymentReader.ReadHistory(50);
            var deployments=deploymentReader.ReadLatest();
            return Results.Ok(new {checkedAt=DateTimeOffset.UtcNow,database="healthy",articles,published,lifecycle,types,users=await db.Users.CountAsync(token),mediaFiles,mediaBytes,diskFreeBytes=drive.AvailableFreeSpace,productionHealth=healthReader.Read(),deployments,deploymentConsistency=DeploymentSnapshotReader.MeasureConsistency(deployments),deploymentHistory=deploymentHistory.Take(12),deploymentReliability=DeploymentSnapshotReader.Measure(deploymentHistory)});
        }).RequireAuthorization(AuthorizationPolicies.ManageUsers).WithTags("Operations");return endpoints;
    }
}

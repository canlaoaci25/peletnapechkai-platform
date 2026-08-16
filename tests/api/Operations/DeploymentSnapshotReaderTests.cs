using Microsoft.Extensions.Configuration;
using Peletnapechkai.Api.Infrastructure.Operations;
namespace Peletnapechkai.Api.Tests.Operations;
public sealed class DeploymentSnapshotReaderTests : IDisposable
{
    private readonly string root=Path.Combine(Path.GetTempPath(),$"boecl-deploy-{Guid.NewGuid():N}");
    public DeploymentSnapshotReaderTests()=>Directory.CreateDirectory(root);
    [Fact] public void ReadLatest_ValidSnapshots_ReturnsNewestFirst(){File.WriteAllText(Path.Combine(root,"latest-production-web.json"),"""{"SchemaVersion":1,"Environment":"Production","Component":"Web","Status":"Succeeded","Commit":"abc123","Message":"Gates passed","StartedAt":"2026-08-16T15:00:00Z","UpdatedAt":"2026-08-16T15:02:00Z","DurationSeconds":120}""");File.WriteAllText(Path.Combine(root,"latest-staging-api.json"),"""{"SchemaVersion":1,"Environment":"Staging","Component":"Api","Status":"RolledBack","Commit":"def456","Message":"Health failed","StartedAt":"2026-08-16T14:00:00Z","UpdatedAt":"2026-08-16T14:01:00Z","DurationSeconds":60}""");var result=CreateReader().ReadLatest();Assert.Equal(2,result.Length);Assert.Equal("Production",result[0].Environment);Assert.Equal("RolledBack",result[1].Status);}
    [Fact] public void ReadLatest_InvalidOrOversizedSnapshots_AreIgnored(){File.WriteAllText(Path.Combine(root,"latest-production-api.json"),"not-json");File.WriteAllText(Path.Combine(root,"latest-staging-web.json"),new string('x',70_000));Assert.Empty(CreateReader().ReadLatest());}
    [Fact] public void ReadHistory_ReturnsBoundedNewestValidDeployments(){for(var i=0;i<15;i++)File.WriteAllText(Path.Combine(root,$"deployment-run{i}.json"),$$"""{"SchemaVersion":2,"DeploymentId":"run{{i}}","Environment":"Production","Component":"Web","Status":"{{(i==14?"RolledBack":"Succeeded")}}","Commit":"abc{{i}}","Message":"Gate result","StartedAt":"2026-08-16T15:00:00Z","UpdatedAt":"2026-08-16T15:{{i:D2}}:00Z","DurationSeconds":{{i}}}""");var result=CreateReader().ReadHistory();Assert.Equal(12,result.Length);Assert.Equal("run14",result[0].DeploymentId);Assert.Equal("RolledBack",result[0].Status);Assert.DoesNotContain(result,x=>x.DeploymentId=="run0");}
    private DeploymentSnapshotReader CreateReader()=>new(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"Operations:DeploymentJournalPath",root}}).Build());
    public void Dispose()=>Directory.Delete(root,true);
}

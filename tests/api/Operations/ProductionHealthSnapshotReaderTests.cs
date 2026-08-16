using Microsoft.Extensions.Configuration;
using Peletnapechkai.Api.Infrastructure.Operations;

namespace Peletnapechkai.Api.Tests.Operations;

public sealed class ProductionHealthSnapshotReaderTests : IDisposable
{
    private readonly string path = Path.Combine(Path.GetTempPath(), $"boecl-health-{Guid.NewGuid():N}.json");
    private readonly DateTimeOffset now = new(2026, 8, 16, 15, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Read_ValidFreshSnapshot_ReportsDeploymentGates()
    {
        File.WriteAllText(path, """{"CheckedAt":"2026-08-16T14:55:00Z","Healthy":true,"Services":[{"Status":"Running"},{"Status":"Stopped"}],"Endpoints":[{"Status":200},{"Status":503}],"FreeDiskGb":152.08,"CertificateDaysRemaining":81,"Failures":["Web service stopped"]}""");

        var result = CreateReader().Read();

        Assert.True(result.Available);
        Assert.True(result.Healthy);
        Assert.False(result.Stale);
        Assert.Equal((1, 2), (result.ServicesHealthy, result.ServicesTotal));
        Assert.Equal((1, 2), (result.EndpointsHealthy, result.EndpointsTotal));
        Assert.Equal(81, result.CertificateDaysRemaining);
        Assert.Single(result.Failures);
    }

    [Fact]
    public void Read_OldSnapshot_IsStaleAndNeverReportedHealthy()
    {
        File.WriteAllText(path, """{"CheckedAt":"2026-08-16T14:00:00Z","Healthy":true,"Services":[],"Endpoints":[],"Failures":[]}""");
        var result = CreateReader().Read();
        Assert.True(result.Stale);
        Assert.False(result.Healthy);
    }

    [Fact]
    public void Read_InvalidSnapshot_FailsClosedWithoutThrowing()
    {
        File.WriteAllText(path, "not-json");
        var result = CreateReader().Read();
        Assert.False(result.Available);
        Assert.False(result.Healthy);
    }

    private ProductionHealthSnapshotReader CreateReader()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Operations:ProductionHealthSnapshotPath"] = path }).Build();
        return new(config, new FixedTimeProvider(now));
    }

    public void Dispose() { if (File.Exists(path)) File.Delete(path); }

    private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => value;
    }
}

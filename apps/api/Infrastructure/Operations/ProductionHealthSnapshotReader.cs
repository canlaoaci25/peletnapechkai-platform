using System.Text.Json;

namespace Peletnapechkai.Api.Infrastructure.Operations;

public sealed record ProductionHealthSnapshot(
    DateTimeOffset? CheckedAt,
    bool Available,
    bool Healthy,
    bool Stale,
    int ServicesHealthy,
    int ServicesTotal,
    int EndpointsHealthy,
    int EndpointsTotal,
    decimal? FreeDiskGb,
    int? CertificateDaysRemaining,
    string[] Failures);

public sealed class ProductionHealthSnapshotReader(IConfiguration configuration, TimeProvider timeProvider)
{
    private const int MaximumSnapshotBytes = 256 * 1024;
    private static readonly TimeSpan StaleAfter = TimeSpan.FromMinutes(20);
    private readonly string snapshotPath = configuration["Operations:ProductionHealthSnapshotPath"]
        ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Peletnapechkai", "Health", "latest.json");

    public ProductionHealthSnapshot Read()
    {
        try
        {
            var file = new FileInfo(snapshotPath);
            if (!file.Exists || file.Length is <= 0 or > MaximumSnapshotBytes) return Unavailable();
            using var stream = new FileStream(snapshotPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var document = JsonDocument.Parse(stream);
            var root = document.RootElement;
            var checkedAt = root.TryGetProperty("CheckedAt", out var checkedElement)
                && checkedElement.ValueKind == JsonValueKind.String
                && DateTimeOffset.TryParse(checkedElement.GetString(), out var parsed) ? parsed : (DateTimeOffset?)null;
            var services = ReadChecks(root, "Services", "Status", value => string.Equals(value, "Running", StringComparison.OrdinalIgnoreCase));
            var endpoints = ReadChecks(root, "Endpoints", "Status", value => int.TryParse(value, out var status) && status is >= 200 and < 400);
            var failures = root.TryGetProperty("Failures", out var failureElement) && failureElement.ValueKind == JsonValueKind.Array
                ? failureElement.EnumerateArray().Where(x => x.ValueKind == JsonValueKind.String).Select(x => x.GetString()!).Take(10).ToArray()
                : [];
            var reportedHealthy = root.TryGetProperty("Healthy", out var healthyElement) && healthyElement.ValueKind == JsonValueKind.True;
            var stale = !checkedAt.HasValue || timeProvider.GetUtcNow() - checkedAt.Value.ToUniversalTime() > StaleAfter;
            return new(checkedAt, true, reportedHealthy && !stale, stale, services.Healthy, services.Total,
                endpoints.Healthy, endpoints.Total, ReadDecimal(root, "FreeDiskGb"), ReadInt(root, "CertificateDaysRemaining"), failures);
        }
        catch (IOException) { return Unavailable(); }
        catch (UnauthorizedAccessException) { return Unavailable(); }
        catch (JsonException) { return Unavailable(); }
    }

    private static (int Healthy, int Total) ReadChecks(JsonElement root, string property, string valueProperty, Func<string?, bool> isHealthy)
    {
        if (!root.TryGetProperty(property, out var items) || items.ValueKind != JsonValueKind.Array) return (0, 0);
        var total = 0; var healthy = 0;
        foreach (var item in items.EnumerateArray())
        {
            total++;
            if (item.TryGetProperty(valueProperty, out var value) && isHealthy(value.ToString())) healthy++;
        }
        return (healthy, total);
    }

    private static decimal? ReadDecimal(JsonElement root, string property) =>
        root.TryGetProperty(property, out var value) && value.TryGetDecimal(out var parsed) ? parsed : null;
    private static int? ReadInt(JsonElement root, string property) =>
        root.TryGetProperty(property, out var value) && value.TryGetInt32(out var parsed) ? parsed : null;
    private static ProductionHealthSnapshot Unavailable() => new(null, false, false, true, 0, 0, 0, 0, null, null, []);
}

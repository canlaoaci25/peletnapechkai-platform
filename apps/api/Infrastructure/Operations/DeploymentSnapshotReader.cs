using System.Text.Json;

namespace Peletnapechkai.Api.Infrastructure.Operations;

public sealed record DeploymentSnapshot(string DeploymentId, string Environment, string Component, string Status, string Commit,
    string Message, DateTimeOffset StartedAt, DateTimeOffset UpdatedAt, int DurationSeconds);
public sealed record DeploymentReliability(int SampleSize, int Successful, int Recovered, int Failed, int SuccessRate,
    int MedianDurationSeconds, int P95DurationSeconds, int HealthyStreak, int Stalled, string State);

public sealed class DeploymentSnapshotReader(IConfiguration configuration)
{
    private const int MaximumSnapshotBytes = 64 * 1024;
    private readonly string root = configuration["Operations:DeploymentJournalPath"]
        ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Peletnapechkai", "Deployments");

    public DeploymentSnapshot[] ReadLatest() => new[] { "staging-web", "staging-api", "production-web", "production-api" }
        .Select(Read).Where(x => x is not null).Cast<DeploymentSnapshot>().OrderByDescending(x => x.UpdatedAt).ToArray();

    public DeploymentSnapshot[] ReadHistory(int limit = 12)
    {
        try
        {
            return Directory.Exists(root)
                ? Directory.EnumerateFiles(root, "deployment-*.json", SearchOption.TopDirectoryOnly)
                    .Select(ReadFile).Where(x => x is not null).Cast<DeploymentSnapshot>()
                    .OrderByDescending(x => x.UpdatedAt).Take(Math.Clamp(limit, 1, 50)).ToArray()
                : [];
        }
        catch (IOException) { return []; }
        catch (UnauthorizedAccessException) { return []; }
    }

    public static DeploymentReliability Measure(IEnumerable<DeploymentSnapshot> snapshots, DateTimeOffset? checkedAt = null)
    {
        var items = snapshots.OrderByDescending(x => x.UpdatedAt).Take(50).ToArray();
        var completed = items.Where(x => x.Status is "Succeeded" or "RolledBack" or "RollbackFailed" or "Failed")
            .OrderByDescending(x => x.UpdatedAt).Take(50).ToArray();
        var staleBefore = (checkedAt ?? DateTimeOffset.UtcNow).AddMinutes(-15);
        var stalled = items.Count(x => (x.Status is "Started" or "Verifying") && x.UpdatedAt < staleBefore);
        var successful = completed.Count(x => x.Status == "Succeeded");
        var recovered = completed.Count(x => x.Status == "RolledBack");
        var failed = completed.Length - successful - recovered;
        var durations = completed.Select(x => Math.Max(0, x.DurationSeconds)).Order().ToArray();
        var healthyStreak = completed.TakeWhile(x => x.Status == "Succeeded").Count();
        var rate = completed.Length == 0 ? 0 : (int)Math.Round(successful * 100d / completed.Length);
        var p95Index = durations.Length == 0 ? 0 : (int)Math.Ceiling(durations.Length * .95) - 1;
        var state = stalled > 0 || failed > 0 || (completed.Length > 0 && rate < 90) ? "AtRisk" : completed.Length == 0 ? "NoData" : recovered > 0 || rate < 100 ? "Watch" : "Healthy";
        return new(completed.Length, successful, recovered, failed, rate,
            durations.Length == 0 ? 0 : durations[(durations.Length - 1) / 2], durations.Length == 0 ? 0 : durations[p95Index], healthyStreak, stalled, state);
    }

    private DeploymentSnapshot? Read(string name)
    {
        try
        {
            return ReadFile(Path.Combine(root, $"latest-{name}.json"));
        }
        catch (IOException) { return null; } catch (UnauthorizedAccessException) { return null; }
    }

    private static DeploymentSnapshot? ReadFile(string path)
    {
        try
        {
            var file = new FileInfo(path);
            if (!file.Exists || file.Length is <= 0 or > MaximumSnapshotBytes) return null;
            using var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var document = JsonDocument.Parse(stream); var value = document.RootElement;
            if (ReadInt(value,"SchemaVersion") is not (1 or 2) || !TryDate(value,"StartedAt",out var started) || !TryDate(value,"UpdatedAt",out var updated)) return null;
            var environment=ReadText(value,"Environment",20); var component=ReadText(value,"Component",20); var status=ReadText(value,"Status",30);
            if (environment is not ("Staging" or "Production") || component is not ("Web" or "Api") || status is null) return null;
            return new(ReadText(value,"DeploymentId",64)??"legacy",environment,component,status,ReadText(value,"Commit",64)??"",ReadText(value,"Message",240)??"",started,updated,ReadInt(value,"DurationSeconds")??0);
        }
        catch (IOException) { return null; } catch (UnauthorizedAccessException) { return null; } catch (JsonException) { return null; }
    }
    private static string? ReadText(JsonElement root,string name,int max) => root.TryGetProperty(name,out var item)&&item.ValueKind==JsonValueKind.String ? item.GetString()?[..Math.Min(item.GetString()!.Length,max)] : null;
    private static int? ReadInt(JsonElement root,string name) => root.TryGetProperty(name,out var item)&&item.TryGetInt32(out var value)?value:null;
    private static bool TryDate(JsonElement root,string name,out DateTimeOffset value) { value=default; return root.TryGetProperty(name,out var item)&&item.ValueKind==JsonValueKind.String&&DateTimeOffset.TryParse(item.GetString(),out value); }
}

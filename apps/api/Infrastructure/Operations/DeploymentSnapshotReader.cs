using System.Text.Json;

namespace Peletnapechkai.Api.Infrastructure.Operations;

public sealed record DeploymentSnapshot(string Environment, string Component, string Status, string Commit,
    string Message, DateTimeOffset StartedAt, DateTimeOffset UpdatedAt, int DurationSeconds);

public sealed class DeploymentSnapshotReader(IConfiguration configuration)
{
    private const int MaximumSnapshotBytes = 64 * 1024;
    private readonly string root = configuration["Operations:DeploymentJournalPath"]
        ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Peletnapechkai", "Deployments");

    public DeploymentSnapshot[] ReadLatest() => new[] { "staging-web", "staging-api", "production-web", "production-api" }
        .Select(Read).Where(x => x is not null).Cast<DeploymentSnapshot>().OrderByDescending(x => x.UpdatedAt).ToArray();

    private DeploymentSnapshot? Read(string name)
    {
        try
        {
            var file = new FileInfo(Path.Combine(root, $"latest-{name}.json"));
            if (!file.Exists || file.Length is <= 0 or > MaximumSnapshotBytes) return null;
            using var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var document = JsonDocument.Parse(stream); var value = document.RootElement;
            if (ReadInt(value,"SchemaVersion") != 1 || !TryDate(value,"StartedAt",out var started) || !TryDate(value,"UpdatedAt",out var updated)) return null;
            var environment=ReadText(value,"Environment",20); var component=ReadText(value,"Component",20); var status=ReadText(value,"Status",30);
            if (environment is not ("Staging" or "Production") || component is not ("Web" or "Api") || status is null) return null;
            return new(environment,component,status,ReadText(value,"Commit",64)??"",ReadText(value,"Message",240)??"",started,updated,ReadInt(value,"DurationSeconds")??0);
        }
        catch (IOException) { return null; } catch (UnauthorizedAccessException) { return null; } catch (JsonException) { return null; }
    }
    private static string? ReadText(JsonElement root,string name,int max) => root.TryGetProperty(name,out var item)&&item.ValueKind==JsonValueKind.String ? item.GetString()?[..Math.Min(item.GetString()!.Length,max)] : null;
    private static int? ReadInt(JsonElement root,string name) => root.TryGetProperty(name,out var item)&&item.TryGetInt32(out var value)?value:null;
    private static bool TryDate(JsonElement root,string name,out DateTimeOffset value) { value=default; return root.TryGetProperty(name,out var item)&&item.ValueKind==JsonValueKind.String&&DateTimeOffset.TryParse(item.GetString(),out value); }
}

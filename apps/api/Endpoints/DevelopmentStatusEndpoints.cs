using System.Text.Json;
using Peletnapechkai.Api.Infrastructure.Identity;

namespace Peletnapechkai.Api.Endpoints;

public static class DevelopmentStatusEndpoints
{
    public static IEndpointRouteBuilder MapDevelopmentStatusEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/admin/development/status",GetAsync).RequireAuthorization(AuthorizationPolicies.ManageUsers).WithTags("Development status");
        return endpoints;
    }
    private static async Task<IResult> GetAsync(IConfiguration configuration,CancellationToken token)
    {
        var path=configuration["DevelopmentStatus:Path"]??Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),"Peletnapechkai","LiveDevelopment","status.json");
        if(!File.Exists(path))return Results.Ok(new{task="Bekleyen Codex görevi yok",phase="Hazır",status="Paused",steps=Array.Empty<string>(),currentStep=0,lastAction="Henüz canlı durum kaydı oluşturulmadı.",commit="",startedAt=(DateTimeOffset?)null,updatedAt=(DateTimeOffset?)null,machine=Environment.MachineName});
        try{await using var stream=File.Open(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite);var value=await JsonSerializer.DeserializeAsync<JsonElement>(stream,cancellationToken:token);return Results.Ok(value);}catch(JsonException){return Results.Problem("Canlı durum dosyası okunamadı.",statusCode:503);}
    }
}

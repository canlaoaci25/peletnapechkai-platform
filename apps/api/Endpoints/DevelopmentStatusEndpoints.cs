using System.Text.Json;
using Peletnapechkai.Api.Infrastructure.Identity;

namespace Peletnapechkai.Api.Endpoints;

public static class DevelopmentStatusEndpoints
{
    public static IEndpointRouteBuilder MapDevelopmentStatusEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/admin/development/status",GetAsync).RequireAuthorization(AuthorizationPolicies.ManageUsers).WithTags("Development status");
        endpoints.MapGet("/api/v1/admin/development/autonomous",GetAutonomousAsync).RequireAuthorization(AuthorizationPolicies.ManageUsers).WithTags("Development status");
        return endpoints;
    }
    private static async Task<IResult> GetAsync(IConfiguration configuration,CancellationToken token)
    {
        var path=configuration["DevelopmentStatus:Path"]??Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),"Peletnapechkai","LiveDevelopment","status.json");
        if(!File.Exists(path))return Results.Ok(new{task="Bekleyen Codex görevi yok",phase="Hazır",status="Paused",steps=Array.Empty<string>(),currentStep=0,lastAction="Henüz canlı durum kaydı oluşturulmadı.",commit="",startedAt=(DateTimeOffset?)null,updatedAt=(DateTimeOffset?)null,machine=Environment.MachineName});
        try{await using var stream=File.Open(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite);var value=await JsonSerializer.DeserializeAsync<JsonElement>(stream,cancellationToken:token);return Results.Ok(value);}catch(JsonException){return Results.Problem("Canlı durum dosyası okunamadı.",statusCode:503);}
    }

    private static async Task<IResult> GetAutonomousAsync(CancellationToken token)
    {
        var root=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),"Peletnapechkai","Autonomous");
        var statePath=Path.Combine(root,"state.json");
        if(!File.Exists(statePath))return Results.Ok(new{enabled=false,cycle=0,status="NotInstalled",focus=(string?)null,lastResult=(string?)null,startedAt=(DateTimeOffset?)null,updatedAt=(DateTimeOffset?)null,events=Array.Empty<object>(),reports=Array.Empty<object>()});
        JsonElement state;
        try{await using var stream=File.Open(statePath,FileMode.Open,FileAccess.Read,FileShare.ReadWrite);state=await JsonSerializer.DeserializeAsync<JsonElement>(stream,cancellationToken:token);}
        catch(JsonException){return Results.Problem("Otonom durum dosyası okunamadı.",statusCode:503);}
        var eventPath=GetString(state,"currentEventLog");
        var events=new List<object>();
        if(!string.IsNullOrWhiteSpace(eventPath)&&File.Exists(eventPath)&&Path.GetFullPath(eventPath).StartsWith(Path.GetFullPath(root)+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))
        {
            foreach(var line in File.ReadLines(eventPath).TakeLast(80))
            {
                try
                {
                    using var document=JsonDocument.Parse(line);var item=document.RootElement;
                    var type=GetString(item,"type");
                    if(type is "turn.completed" or "turn.failed"){events.Add(new{type,text=type=="turn.completed"?"Çevrim tamamlandı.":"Çevrim başarısız oldu.",exitCode=(int?)null});continue;}
                    if(!item.TryGetProperty("item",out var detail))continue;
                    var itemType=GetString(detail,"type");
                    if(itemType=="agent_message")AddSafeEvent(events,"message",GetString(detail,"text"),null);
                    else if(itemType=="command_execution"&&type is "item.started" or "item.completed")
                    {
                        var exitCode=detail.TryGetProperty("exit_code",out var exit)&&exit.ValueKind==JsonValueKind.Number?exit.GetInt32():(int?)null;
                        AddSafeEvent(events,type=="item.started"?"command_started":"command_completed",GetString(detail,"command"),exitCode);
                    }
                }
                catch(JsonException){ }
            }
        }
        var logRoot=Path.Combine(root,"Logs");
        var reports=Directory.Exists(logRoot)?Directory.EnumerateFiles(logRoot,"*-result.txt").Select(path=>new FileInfo(path)).OrderByDescending(file=>file.LastWriteTimeUtc).Take(10)
            .Select(file=>(object)new{id=Path.GetFileNameWithoutExtension(file.Name),createdAt=file.LastWriteTimeUtc,text=SafeText(File.ReadAllText(file.FullName))}).ToArray():Array.Empty<object>();
        return Results.Ok(new{enabled=GetBool(state,"enabled"),cycle=GetInt(state,"currentCycle")??GetInt(state,"cycle")??0,status=GetString(state,"currentStatus")??GetString(state,"lastResult")??"Waiting",focus=SafeText(GetString(state,"currentFocus")),lastResult=SafeText(GetString(state,"lastResult")),startedAt=GetString(state,"currentStartedAt")??GetString(state,"startedAt"),updatedAt=GetString(state,"updatedAt"),events,reports});
    }

    private static void AddSafeEvent(List<object> events,string type,string? text,int? exitCode){var safe=SafeText(text);if(!string.IsNullOrWhiteSpace(safe))events.Add(new{type,text=safe,exitCode});}
    private static string? SafeText(string? value){if(string.IsNullOrWhiteSpace(value))return null;var text=value.Trim()[..Math.Min(value.Trim().Length,4000)];return System.Text.RegularExpressions.Regex.IsMatch(text,"(?i)(password|secret|token|api[-_ ]?key|authorization)")?"[Güvenlik nedeniyle gizlendi]":text;}
    private static string? GetString(JsonElement value,string name)=>value.TryGetProperty(name,out var property)&&property.ValueKind==JsonValueKind.String?property.GetString():null;
    private static bool GetBool(JsonElement value,string name)=>value.TryGetProperty(name,out var property)&&property.ValueKind==JsonValueKind.True;
    private static int? GetInt(JsonElement value,string name)=>value.TryGetProperty(name,out var property)&&property.ValueKind==JsonValueKind.Number?property.GetInt32():null;
}

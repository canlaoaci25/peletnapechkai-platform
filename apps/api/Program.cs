using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Endpoints;
using Peletnapechkai.Api.Infrastructure.Identity;
using Peletnapechkai.Api.Localization;
using Peletnapechkai.Api.Infrastructure.Persistence;
using Peletnapechkai.Api.Infrastructure.Publishing;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddApplicationIdentity(builder.Environment, builder.Configuration);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHostedService<ScheduledPublishingWorker>();

var app = builder.Build();

if (await OwnerBootstrap.TryRunAsync(app, args))
{
    return;
}

if (await StarterContentBootstrap.TryRunAsync(app, args))
{
    return;
}

app.UseExceptionHandler();
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health");

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseAntiforgery();

app.MapAuthEndpoints();
app.MapUserManagementEndpoints();
app.MapEditorialEndpoints();
app.MapLocaleManagementEndpoints();
app.MapAutomationEndpoints();
app.MapSupportingContentEndpoints();
app.MapSystemStatusEndpoints();
app.MapKnowledgeEndpoints();
app.MapPublicContentEndpoints();

app.MapGet("/api/v1/locales", async (PublishingDbContext database, CancellationToken cancellationToken) =>
    Results.Ok(new
    {
        defaultLocale = SupportedLocales.Default,
        locales = await database.Locales
            .AsNoTracking()
            .Where(locale => locale.IsEnabled)
            .OrderByDescending(locale => locale.IsDefault)
            .ThenBy(locale => locale.Code)
            .Select(locale => new
            {
                locale.Code,
                locale.LanguageCode,
                locale.DisplayName,
                locale.NativeName,
                region = locale.Region.Code
            })
            .ToListAsync(cancellationToken)
    }))
.WithName("GetSupportedLocales")
.WithSummary("Returns the locales enabled for the first publishing release.");

app.Run();

public partial class Program;

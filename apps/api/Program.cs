using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Endpoints;
using Peletnapechkai.Api.Infrastructure.Identity;
using Peletnapechkai.Api.Localization;
using Peletnapechkai.Api.Infrastructure.Persistence;
using Peletnapechkai.Api.Infrastructure.Publishing;
using Peletnapechkai.Api.Infrastructure.Automation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddApplicationIdentity(builder.Environment, builder.Configuration);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHostedService<ScheduledPublishingWorker>();
builder.Services.AddHostedService<AutomaticContentWorker>();

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
app.MapAutomationWorkerEndpoints();
app.MapSupportingContentEndpoints();
app.MapSystemStatusEndpoints();
app.MapKnowledgeEndpoints();
app.MapPublicContentEndpoints();
app.MapHomepageEndpoints();
app.MapDevelopmentStatusEndpoints();
app.MapMemberAccountEndpoints();
app.MapTrafficGrowthEndpoints();

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
                region = locale.Region.Code,
                countries = locale.Countries
                    .Where(link => link.IsEnabled && link.Country.IsEnabled)
                    .OrderByDescending(link => link.CountryId == locale.RegionId)
                    .ThenBy(link => link.Country.Code)
                    .Select(link => link.Country.Code)
                    .ToArray()
            })
            .ToListAsync(cancellationToken)
    }))
.WithName("GetSupportedLocales")
.WithSummary("Returns the locales enabled for the first publishing release.");

app.Run();

public partial class Program;

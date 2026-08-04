using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Localization;
using Peletnapechkai.Api.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddPersistence(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health");

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

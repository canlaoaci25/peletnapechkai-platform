using Peletnapechkai.Api.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health");

app.MapGet("/api/v1/locales", () => Results.Ok(new
{
    defaultLocale = SupportedLocales.Default,
    locales = SupportedLocales.All
}))
.WithName("GetSupportedLocales")
.WithSummary("Returns the locales enabled for the first publishing release.");

app.Run();

public partial class Program;

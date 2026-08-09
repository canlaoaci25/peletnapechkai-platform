using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Domain.Auditing;
using Peletnapechkai.Api.Domain.Identity;
using Peletnapechkai.Api.Domain.Localization;
using Peletnapechkai.Api.Infrastructure.Identity;
using Peletnapechkai.Api.Infrastructure.Persistence;

namespace Peletnapechkai.Api.Endpoints;

public static class LocaleManagementEndpoints
{
    public static IEndpointRouteBuilder MapLocaleManagementEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/admin/locales").WithTags("Locale management").RequireAuthorization(AuthorizationPolicies.ManageUsers);
        group.MapGet("/", ListAsync);
        group.MapPost("/", CreateAsync).ValidateAntiforgery();
        group.MapPut("/{localeId:guid}", UpdateAsync).ValidateAntiforgery();
        group.MapPut("/{localeId:guid}/countries/{countryCode}", UpdateCountryAsync).ValidateAntiforgery();
        return endpoints;
    }

    private static async Task<IResult> ListAsync(PublishingDbContext database, CancellationToken token) =>
        Results.Ok(await database.Locales.AsNoTracking()
            .OrderByDescending(locale => locale.IsDefault).ThenBy(locale => locale.Code)
            .Select(locale => new
            {
                locale.Id, locale.Code, locale.LanguageCode, locale.DisplayName, locale.NativeName,
                locale.IsDefault, locale.IsEnabled,
                articleCount = locale.ArticleLocalizations.Count,
                countries = locale.Countries.OrderByDescending(item => item.CountryId == locale.RegionId).ThenBy(item => item.Country.Name)
                    .Select(item => new { code = item.Country.Code, item.Country.Name, item.Country.CurrencyCode, item.IsRequired, item.IsEnabled, isPrimary = item.CountryId == locale.RegionId })
            }).ToListAsync(token));

    private static async Task<IResult> CreateAsync(CreateLocaleRequest request, System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext database, CancellationToken token)
    {
        var actor = await users.GetUserAsync(principal);
        if (actor is null) return Results.Unauthorized();
        CultureInfo culture;
        try { culture = CultureInfo.GetCultureInfo(request.Code.Trim()); }
        catch (CultureNotFoundException) { return Results.ValidationProblem(new Dictionary<string, string[]> { ["code"] = ["Geçerli bir dil-bölge kodu kullanın (ör. fr-FR)."] }); }
        if (culture.IsNeutralCulture) return Results.ValidationProblem(new Dictionary<string, string[]> { ["code"] = ["Dil kodu ülke içermelidir (ör. fr-FR)."] });
        var code = culture.Name;
        if (await database.Locales.AnyAsync(item => item.Code == code, token)) return Results.Conflict(new { message = "Bu dil zaten kayıtlı." });
        var primaryInfo = new RegionInfo(culture.Name);
        var countryInfos = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            .Where(item => item.TwoLetterISOLanguageName == culture.TwoLetterISOLanguageName)
            .Select(item => new RegionInfo(item.Name)).GroupBy(item => item.TwoLetterISORegionName).Select(group => group.First())
            .OrderBy(item => item.EnglishName).ToArray();
        var countryCodes = countryInfos.Select(item => item.TwoLetterISORegionName).ToArray();
        var countries = await database.Regions.Where(item => countryCodes.Contains(item.Code)).ToDictionaryAsync(item => item.Code, token);
        foreach (var info in countryInfos.Where(info => !countries.ContainsKey(info.TwoLetterISORegionName)))
        {
            var country = new Region(Guid.CreateVersion7(), info.TwoLetterISORegionName, info.NativeName, info.ISOCurrencySymbol);
            database.Regions.Add(country); countries[country.Code] = country;
        }
        var primary = countries[primaryInfo.TwoLetterISORegionName];
        var locale = new Locale(Guid.CreateVersion7(), code, culture.TwoLetterISOLanguageName, primary,
            string.IsNullOrWhiteSpace(request.DisplayName) ? culture.EnglishName : request.DisplayName,
            string.IsNullOrWhiteSpace(request.NativeName) ? culture.NativeName : request.NativeName, false);
        locale.Update(locale.DisplayName, locale.NativeName, false);
        foreach (var country in countries.Values) locale.Countries.Add(new LocaleCountry(locale, country, true));
        database.Locales.Add(locale);
        database.AuditLogs.Add(new AuditLog(actor.Id, "localization.locale_created", nameof(Locale), locale.Id, JsonSerializer.Serialize(new { locale.Code, countries = countries.Count }), DateTimeOffset.UtcNow));
        await database.SaveChangesAsync(token);
        return Results.Created($"/api/v1/admin/locales/{locale.Id}", new { locale.Id, locale.Code });
    }

    private static async Task<IResult> UpdateAsync(Guid localeId, UpdateLocaleRequest request, System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext database, CancellationToken token)
    {
        var actor = await users.GetUserAsync(principal); var locale = await database.Locales.SingleOrDefaultAsync(item => item.Id == localeId, token);
        if (actor is null) return Results.Unauthorized(); if (locale is null) return Results.NotFound();
        if (locale.IsDefault && !request.IsEnabled) return Results.Conflict(new { message = "Varsayılan dil pasifleştirilemez." });
        locale.Update(request.DisplayName, request.NativeName, request.IsEnabled);
        database.AuditLogs.Add(new AuditLog(actor.Id, "localization.locale_updated", nameof(Locale), locale.Id, JsonSerializer.Serialize(new { request.IsEnabled }), DateTimeOffset.UtcNow));
        await database.SaveChangesAsync(token); return Results.Ok(new { locale.Id, locale.IsEnabled });
    }

    private static async Task<IResult> UpdateCountryAsync(Guid localeId, string countryCode, UpdateCountryRequest request, System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext database, CancellationToken token)
    {
        var actor = await users.GetUserAsync(principal); if (actor is null) return Results.Unauthorized();
        var item = await database.LocaleCountries.Include(link => link.Country).SingleOrDefaultAsync(link => link.LocaleId == localeId && link.Country.Code == countryCode.ToUpperInvariant(), token);
        if (item is null) return Results.NotFound(); item.SetEnabled(request.IsEnabled);
        database.AuditLogs.Add(new AuditLog(actor.Id, "localization.country_toggled", nameof(LocaleCountry), localeId, JsonSerializer.Serialize(new { countryCode, request.IsEnabled }), DateTimeOffset.UtcNow));
        await database.SaveChangesAsync(token); return Results.Ok(new { item.LocaleId, item.CountryId, item.IsEnabled });
    }

    private sealed record CreateLocaleRequest(string Code, string? DisplayName, string? NativeName);
    private sealed record UpdateLocaleRequest(string DisplayName, string NativeName, bool IsEnabled);
    private sealed record UpdateCountryRequest(bool IsEnabled);
}

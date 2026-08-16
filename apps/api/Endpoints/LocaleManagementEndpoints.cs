using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Domain.Auditing;
using Peletnapechkai.Api.Domain.Content;
using Peletnapechkai.Api.Domain.Identity;
using Peletnapechkai.Api.Domain.Localization;
using Peletnapechkai.Api.Infrastructure.Identity;
using Peletnapechkai.Api.Infrastructure.Persistence;
using Peletnapechkai.Api.Localization;

namespace Peletnapechkai.Api.Endpoints;

public static class LocaleManagementEndpoints
{
    public static IEndpointRouteBuilder MapLocaleManagementEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/admin/locales").WithTags("Locale management").RequireAuthorization(AuthorizationPolicies.ManageUsers);
        group.MapGet("/", ListAsync);
        group.MapGet("/catalog", Catalog);
        group.MapPost("/", CreateAsync).ValidateAntiforgery();
        group.MapPut("/{localeId:guid}", UpdateAsync).ValidateAntiforgery();
        group.MapPut("/{localeId:guid}/countries/{countryCode}", UpdateCountryAsync).ValidateAntiforgery();
        return endpoints;
    }

    private static IResult Catalog()
    {
        var items = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            .Where(culture => !string.IsNullOrWhiteSpace(culture.Name))
            .GroupBy(culture => culture.Name, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .Select(culture =>
            {
                var region = new RegionInfo(culture.Name);
                return new { code = culture.Name, displayName = culture.EnglishName, nativeName = culture.NativeName, countryCode = region.TwoLetterISORegionName, countryName = region.NativeName };
            })
            .OrderBy(item => item.displayName)
            .ToArray();
        return Results.Ok(items);
    }

    private static async Task<IResult> ListAsync(PublishingDbContext database, CancellationToken token)
    {
        var sourcePublishedCount = await database.ArticleLocalizations.AsNoTracking()
            .CountAsync(article => article.Locale.IsDefault && article.Status == PublicationStatus.Published, token);
        var sourceCategoryCount = await database.Categories.AsNoTracking()
            .CountAsync(category => category.Locale.IsDefault &&
                category.Articles.Any(article => article.Status == PublicationStatus.Published), token);
        var locales = await database.Locales.AsNoTracking()
            .OrderByDescending(locale => locale.IsDefault).ThenBy(locale => locale.Code)
            .Select(locale => new
            {
                locale.Id, locale.Code, locale.LanguageCode, locale.DisplayName, locale.NativeName,
                locale.IsDefault, locale.IsEnabled,
                articleCount = locale.ArticleLocalizations.Count,
                publishedCount = locale.ArticleLocalizations.Count(article => article.Status == PublicationStatus.Published),
                draftCount = locale.ArticleLocalizations.Count(article => article.Status == PublicationStatus.Draft),
                sourcePublishedCount,
                sourceCategoryCount,
                missingTranslationCount = locale.IsDefault ? 0 : database.ArticleLocalizations.Count(source =>
                    source.Locale.IsDefault && source.Status == PublicationStatus.Published &&
                    !database.ArticleLocalizations.Any(translation => translation.ArticleGroupId == source.ArticleGroupId &&
                        translation.LocaleId == locale.Id && translation.Status != PublicationStatus.Archived)),
                reviewPendingCount = locale.IsDefault ? 0 : locale.ArticleLocalizations.Count(article =>
                    (article.Status == PublicationStatus.Draft || article.Status == PublicationStatus.Published) &&
                    !database.ArticleQualityChecklists.Any(checklist => checklist.ArticleLocalizationId == article.Id && checklist.TranslationReviewed)),
                staleTranslationCount = locale.IsDefault ? 0 : locale.ArticleLocalizations.Count(translation =>
                    (translation.Status == PublicationStatus.Draft || translation.Status == PublicationStatus.Published) &&
                    database.ArticleLocalizations.Any(source => source.ArticleGroupId == translation.ArticleGroupId &&
                        source.Locale.IsDefault && source.Status == PublicationStatus.Published && source.UpdatedAt > translation.UpdatedAt)),
                linkedCategoryCount = locale.IsDefault ? sourceCategoryCount : database.Categories.Count(category =>
                    category.LocaleId == locale.Id && category.SourceCategoryId != null &&
                    category.SourceCategory!.Articles.Any(article => article.Status == PublicationStatus.Published)),
                missingCategoryCount = locale.IsDefault ? 0 : database.Categories.Count(source =>
                    source.Locale.IsDefault && source.Articles.Any(article => article.Status == PublicationStatus.Published) &&
                    !database.Categories.Any(translation => translation.LocaleId == locale.Id && translation.SourceCategoryId == source.Id)),
                countries = locale.Countries.OrderByDescending(item => item.CountryId == locale.RegionId).ThenBy(item => item.Country.Name)
                    .Select(item => new { code = item.Country.Code, item.Country.Name, item.Country.CurrencyCode, item.IsRequired, item.IsEnabled, isPrimary = item.CountryId == locale.RegionId })
            }).ToListAsync(token);
        return Results.Ok(locales);
    }

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
        foreach (var country in countries.Values)
        {
            var isPrimary = country.Id == primary.Id;
            locale.Countries.Add(new LocaleCountry(locale, country, isPrimary, isPrimary));
        }
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
        if (request.IsEnabled && !SupportedLocales.Contains(locale.Code))
            return Results.Conflict(new { message = "Bu dilin public arayüz paketi henüz hazır değil. Sözlük ve rota kalite kapıları tamamlanmadan etkinleştirilemez." });
        if (request.IsEnabled)
        {
            var enabledCountryIds = await database.LocaleCountries.AsNoTracking()
                .Where(link => link.LocaleId == locale.Id && link.IsEnabled).Select(link => link.CountryId).ToArrayAsync(token);
            var conflicts = await database.LocaleCountries.AsNoTracking()
                .Where(link => link.LocaleId != locale.Id && link.Locale.IsEnabled && link.IsEnabled && enabledCountryIds.Contains(link.CountryId))
                .Select(link => link.Country.Code).Distinct().Order().ToArrayAsync(token);
            if (conflicts.Length > 0)
                return Results.Conflict(new { message = $"Şu ülkeler başka bir etkin dile bağlı: {string.Join(", ", conflicts)}." });
        }
        locale.Update(request.DisplayName, request.NativeName, request.IsEnabled);
        database.AuditLogs.Add(new AuditLog(actor.Id, "localization.locale_updated", nameof(Locale), locale.Id, JsonSerializer.Serialize(new { request.IsEnabled }), DateTimeOffset.UtcNow));
        await database.SaveChangesAsync(token); return Results.Ok(new { locale.Id, locale.IsEnabled });
    }

    private static async Task<IResult> UpdateCountryAsync(Guid localeId, string countryCode, UpdateCountryRequest request, System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext database, CancellationToken token)
    {
        var actor = await users.GetUserAsync(principal); if (actor is null) return Results.Unauthorized();
        var item = await database.LocaleCountries.Include(link => link.Country).SingleOrDefaultAsync(link => link.LocaleId == localeId && link.Country.Code == countryCode.ToUpperInvariant(), token);
        if (item is null) return Results.NotFound();
        if (request.IsEnabled)
        {
            var conflict = await database.LocaleCountries.AsNoTracking().AnyAsync(link =>
                link.LocaleId != localeId && link.CountryId == item.CountryId && link.IsEnabled && link.Locale.IsEnabled, token);
            if (conflict) return Results.Conflict(new { message = "Bu ülke zaten başka bir etkin dile bağlı." });
        }
        item.SetEnabled(request.IsEnabled);
        database.AuditLogs.Add(new AuditLog(actor.Id, "localization.country_toggled", nameof(LocaleCountry), localeId, JsonSerializer.Serialize(new { countryCode, request.IsEnabled }), DateTimeOffset.UtcNow));
        await database.SaveChangesAsync(token); return Results.Ok(new { item.LocaleId, item.CountryId, item.IsEnabled });
    }

    private sealed record CreateLocaleRequest(string Code, string? DisplayName, string? NativeName);
    private sealed record UpdateLocaleRequest(string DisplayName, string NativeName, bool IsEnabled);
    private sealed record UpdateCountryRequest(bool IsEnabled);
}

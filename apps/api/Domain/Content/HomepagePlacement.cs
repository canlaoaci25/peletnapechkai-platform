using Peletnapechkai.Api.Domain.Localization;

namespace Peletnapechkai.Api.Domain.Content;

public sealed class HomepagePlacement
{
    private HomepagePlacement() { }
    public HomepagePlacement(Locale locale, ArticleLocalization article, string section, int position, DateTimeOffset now)
    {
        if (article.LocaleId != locale.Id || article.Status != PublicationStatus.Published) throw new InvalidOperationException("Homepage placements require a published article in the same locale.");
        if (section is not ("Lead" or "Editors")) throw new ArgumentOutOfRangeException(nameof(section));
        Id = Guid.CreateVersion7(); Locale = locale; LocaleId = locale.Id; ArticleLocalization = article; ArticleLocalizationId = article.Id;
        Section = section; Position = Math.Clamp(position, 0, 7); CreatedAt = now;
    }
    public Guid Id { get; private set; }
    public Guid LocaleId { get; private set; }
    public Locale Locale { get; private set; } = null!;
    public Guid ArticleLocalizationId { get; private set; }
    public ArticleLocalization ArticleLocalization { get; private set; } = null!;
    public string Section { get; private set; } = string.Empty;
    public int Position { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}

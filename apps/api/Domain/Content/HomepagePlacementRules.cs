namespace Peletnapechkai.Api.Domain.Content;

public static class HomepagePlacementRules
{
    public static string? Validate(IReadOnlyCollection<(string Section, int Position, Guid ArticleId)> placements)
    {
        if (placements.Count > 5) return "En fazla beş ana sayfa yerleşimi kaydedilebilir.";
        if (placements.Any(item => item.Section is not ("Lead" or "Editors"))) return "Desteklenmeyen ana sayfa bölümü.";
        if (placements.Count(item => item.Section == "Lead") > 1) return "En fazla bir lider içerik seçilebilir.";
        if (placements.Count(item => item.Section == "Editors") > 4) return "En fazla dört editör seçimi yapılabilir.";
        if (placements.Select(item => item.ArticleId).Distinct().Count() != placements.Count) return "Aynı içerik birden fazla bölüme eklenemez.";
        if (placements.GroupBy(item => (item.Section, item.Position)).Any(group => group.Count() > 1)) return "Aynı bölüm ve pozisyon birden fazla kullanılamaz.";
        if (placements.Any(item => item.Position is < 0 or > 7)) return "Yerleşim pozisyonu 0-7 arasında olmalıdır.";
        return null;
    }
}

namespace Peletnapechkai.Api.Domain.Content;

public static class PublicationQualityGate
{
    public static IReadOnlyList<string> Missing(ArticleQualityChecklist? checklist)
    {
        if (checklist is null) return All;
        var missing = new List<string>(8);
        if (!checklist.TitleAndSummary) missing.Add("titleAndSummary");
        if (!checklist.SourcesVerified) missing.Add("sourcesVerified");
        if (!checklist.AuthorAndTaxonomy) missing.Add("authorAndTaxonomy");
        if (!checklist.SeoMetadata) missing.Add("seoMetadata");
        if (!checklist.CoverAccessibility) missing.Add("coverAccessibility");
        if (!checklist.CommercialDisclosure) missing.Add("commercialDisclosure");
        if (!checklist.TranslationReviewed) missing.Add("translationReviewed");
        if (!checklist.LegalEditorialReview) missing.Add("legalEditorialReview");
        return missing;
    }

    private static readonly string[] All = ["titleAndSummary", "sourcesVerified", "authorAndTaxonomy", "seoMetadata", "coverAccessibility", "commercialDisclosure", "translationReviewed", "legalEditorialReview"];
}

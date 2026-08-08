namespace Peletnapechkai.Api.Domain.Content;

public sealed class ArticleQualityChecklist
{
    private ArticleQualityChecklist(){}
    public ArticleQualityChecklist(ArticleLocalization article){ArgumentNullException.ThrowIfNull(article);Article=article;ArticleLocalizationId=article.Id;}
    public Guid ArticleLocalizationId{get;private set;}public ArticleLocalization Article{get;private set;}=null!;public bool TitleAndSummary{get;private set;}public bool SourcesVerified{get;private set;}public bool AuthorAndTaxonomy{get;private set;}public bool SeoMetadata{get;private set;}public bool CoverAccessibility{get;private set;}public bool CommercialDisclosure{get;private set;}public bool TranslationReviewed{get;private set;}public bool LegalEditorialReview{get;private set;}public Guid? UpdatedByUserId{get;private set;}public DateTimeOffset? UpdatedAt{get;private set;}
    public bool IsComplete=>TitleAndSummary&&SourcesVerified&&AuthorAndTaxonomy&&SeoMetadata&&CoverAccessibility&&CommercialDisclosure&&TranslationReviewed&&LegalEditorialReview;
    public void Update(bool titleAndSummary,bool sourcesVerified,bool authorAndTaxonomy,bool seoMetadata,bool coverAccessibility,bool commercialDisclosure,bool translationReviewed,bool legalEditorialReview,Guid actor,DateTimeOffset now){TitleAndSummary=titleAndSummary;SourcesVerified=sourcesVerified;AuthorAndTaxonomy=authorAndTaxonomy;SeoMetadata=seoMetadata;CoverAccessibility=coverAccessibility;CommercialDisclosure=commercialDisclosure;TranslationReviewed=translationReviewed;LegalEditorialReview=legalEditorialReview;UpdatedByUserId=actor;UpdatedAt=now;}
}

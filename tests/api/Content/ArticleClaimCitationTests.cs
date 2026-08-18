using Peletnapechkai.Api.Domain.Content;
using Peletnapechkai.Api.Domain.Localization;

namespace Peletnapechkai.Api.Tests.Content;

public sealed class ArticleClaimCitationTests
{
    [Fact]
    public void Citation_is_locale_specific_editor_approved_evidence()
    {
        var now=DateTimeOffset.UtcNow;var group=new ArticleGroup(ArticleType.Guide,now);
        var region=new Region(Guid.CreateVersion7(),"TR","Türkiye","TRY");
        var locale=new Locale(Guid.CreateVersion7(),"tr-TR","tr",region,"Turkish","Türkçe",true);
        var article=new ArticleLocalization(group,locale,"ornek","Örnek","Özet","Gövde",now);
        var source=new Source("Resmî kaynak",new Uri("https://example.com/report"),now);var actor=Guid.CreateVersion7();
        var citation=new ArticleClaimCitation(article,source,"  Doğrulanmış iddia.  "," Bölüm 2 ",actor,now);
        Assert.Equal("Doğrulanmış iddia.",citation.Claim);Assert.Equal("Bölüm 2",citation.Locator);
        Assert.Equal(article.Id,citation.ArticleLocalizationId);Assert.Equal(source.Id,citation.SourceId);
        Assert.Equal(actor,citation.ApprovedByUserId);Assert.Contains(citation,article.ClaimCitations);
    }

    [Fact]
    public void Citation_rejects_empty_or_oversized_claims()
    {
        var now=DateTimeOffset.UtcNow;var group=new ArticleGroup(ArticleType.News,now);
        var region=new Region(Guid.CreateVersion7(),"TR","Türkiye","TRY");
        var locale=new Locale(Guid.CreateVersion7(),"tr-TR","tr",region,"Turkish","Türkçe",true);
        var article=new ArticleLocalization(group,locale,"ornek","Örnek","","",now);
        var source=new Source("Kaynak",new Uri("https://example.com"),now);var actor=Guid.CreateVersion7();
        Assert.Throws<ArgumentException>(()=>new ArticleClaimCitation(article,source," ",null,actor,now));
        Assert.Throws<ArgumentOutOfRangeException>(()=>new ArticleClaimCitation(article,source,new string('a',501),null,actor,now));
    }
}

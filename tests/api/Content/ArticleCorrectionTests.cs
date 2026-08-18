using Peletnapechkai.Api.Domain.Content;
using Peletnapechkai.Api.Domain.Localization;

namespace Peletnapechkai.Api.Tests.Content;

public sealed class ArticleCorrectionTests
{
    [Fact]
    public void Correction_is_locale_local_append_only_evidence()
    {
        var now=DateTimeOffset.UtcNow;var region=new Region(Guid.CreateVersion7(),"TR","Türkiye","TRY");
        var locale=new Locale(Guid.CreateVersion7(),"tr-TR","tr",region,"Turkish","Türkçe",true);
        var article=new ArticleLocalization(new ArticleGroup(ArticleType.News,now),locale,"ornek","Başlık","Özet","Gövde",now);
        var actor=Guid.CreateVersion7();var correction=new ArticleCorrection(article,"Tarih düzeltildi","Metindeki tarih güvenilir kaynakla eşleştirildi.",actor,now);
        Assert.Equal(article.Id,correction.ArticleLocalizationId);Assert.Equal(actor,correction.ApprovedByUserId);Assert.Equal(now,correction.CorrectedAt);
    }

    [Theory]
    [InlineData("", "Açıklama")]
    [InlineData("Özet", "")]
    public void Correction_requires_public_summary_and_details(string summary,string details)
    {
        var now=DateTimeOffset.UtcNow;var region=new Region(Guid.CreateVersion7(),"TR","Türkiye","TRY");
        var locale=new Locale(Guid.CreateVersion7(),"tr-TR","tr",region,"Turkish","Türkçe",true);
        var article=new ArticleLocalization(new ArticleGroup(ArticleType.News,now),locale,"ornek","Başlık","Özet","Gövde",now);
        Assert.Throws<ArgumentException>(()=>new ArticleCorrection(article,summary,details,Guid.CreateVersion7(),now));
    }
}

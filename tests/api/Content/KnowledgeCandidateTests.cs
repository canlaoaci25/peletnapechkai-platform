using Peletnapechkai.Api.Domain.Knowledge;
using Peletnapechkai.Api.Domain.Localization;
namespace Peletnapechkai.Api.Tests.Content;
public sealed class KnowledgeCandidateTests
{
    [Fact]
    public void Ai_candidate_requires_human_review()
    {
        var region=new Region(Guid.NewGuid(),"TR","Türkiye","TRY");
        var locale=new Locale(Guid.NewGuid(),"tr-TR","tr",region,"Türkçe","Türkçe",true);
        var item=new KnowledgeCandidate(locale,"Başlık","Doğrulanacak iddia",new Uri("https://example.com/source"),true,Guid.NewGuid(),DateTimeOffset.UtcNow);
        Assert.Equal(KnowledgeReviewStatus.PendingReview,item.Status);
        item.Review(true,Guid.NewGuid(),DateTimeOffset.UtcNow);
        Assert.Equal(KnowledgeReviewStatus.Approved,item.Status);
        Assert.Throws<InvalidOperationException>(()=>item.Review(false,Guid.NewGuid(),DateTimeOffset.UtcNow));
    }
}

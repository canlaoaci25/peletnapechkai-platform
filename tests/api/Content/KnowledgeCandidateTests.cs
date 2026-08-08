using Peletnapechkai.Api.Domain.Knowledge;
using Peletnapechkai.Api.Domain.Localization;
using Peletnapechkai.Api.Domain.Content;
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

    [Fact]
    public void Approved_knowledge_links_only_to_same_locale_and_tracks_freshness()
    {
        var now=DateTimeOffset.UtcNow;var region=new Region(Guid.NewGuid(),"TR","Türkiye","TRY");
        var locale=new Locale(Guid.NewGuid(),"tr-TR","tr",region,"Türkçe","Türkçe",true);
        var otherLocale=new Locale(Guid.NewGuid(),"en-US","en",region,"English","English",false);
        var candidate=new KnowledgeCandidate(locale,"Başlık","İddia",new Uri("https://example.com/source"),false,Guid.NewGuid(),now);
        var article=new ArticleLocalization(new ArticleGroup(ArticleType.Analysis,now),locale,"makale","Makale","Özet","Gövde",now);
        var otherArticle=new ArticleLocalization(new ArticleGroup(ArticleType.Analysis,now),otherLocale,"article","Article","Summary","Body",now);
        Assert.Throws<InvalidOperationException>(()=>new KnowledgeArticleLink(candidate,article,KnowledgeUsePurpose.Evidence,null,now.AddDays(30),Guid.NewGuid(),now));
        candidate.Review(true,Guid.NewGuid(),now.AddMinutes(1));
        Assert.Throws<InvalidOperationException>(()=>new KnowledgeArticleLink(candidate,otherArticle,KnowledgeUsePurpose.Evidence,null,now.AddDays(30),Guid.NewGuid(),now.AddMinutes(2)));
        var link=new KnowledgeArticleLink(candidate,article,KnowledgeUsePurpose.UpdatePrompt,"Güncellik notu",now.AddDays(30),Guid.NewGuid(),now.AddMinutes(2));
        link.Verify(now.AddDays(60),Guid.NewGuid(),now.AddDays(20));
        Assert.Equal(now.AddDays(60),link.ReviewDueAt);Assert.Equal(now.AddDays(20),link.LastVerifiedAt);
    }
}

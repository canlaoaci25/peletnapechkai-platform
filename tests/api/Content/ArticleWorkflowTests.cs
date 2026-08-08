using Peletnapechkai.Api.Domain.Content;
using Peletnapechkai.Api.Domain.Localization;

namespace Peletnapechkai.Api.Tests.Content;

public sealed class ArticleWorkflowTests
{
    [Fact]
    public void NewLocalization_IsAttachedToItsArticleGroup()
    {
        var article = CreateArticle(DateTimeOffset.UtcNow);

        Assert.Contains(article, article.ArticleGroup.Localizations);
    }

    [Fact]
    public void ApprovedArticle_CanBeScheduledAndPublished()
    {
        var now = DateTimeOffset.UtcNow;
        var article = CreateArticle(now);

        article.SubmitForEditorialReview(now.AddMinutes(1));
        article.ApproveEditorialReview(now.AddMinutes(2));
        article.Schedule(now.AddHours(1), now.AddMinutes(3));
        article.Publish(now.AddHours(1));

        Assert.Equal(PublicationStatus.Published, article.Status);
        Assert.Equal(now.AddHours(1), article.PublishedAt);
        Assert.Null(article.ScheduledAt);
    }

    [Fact]
    public void NonDraftArticle_CannotBeEdited()
    {
        var now = DateTimeOffset.UtcNow;
        var article = CreateArticle(now);
        article.SubmitForEditorialReview(now.AddMinutes(1));

        Assert.Throws<InvalidOperationException>(() =>
            article.UpdateDraft("new", "New title", "summary", "body", null, null, now.AddMinutes(2)));
    }

    [Fact]
    public void ReviewArticle_CanReturnToDraft()
    {
        var now = DateTimeOffset.UtcNow;
        var article = CreateArticle(now);
        article.SubmitForEditorialReview(now.AddMinutes(1));
        article.ReturnToDraft(now.AddMinutes(2));

        Assert.Equal(PublicationStatus.Draft, article.Status);
    }

    private static ArticleLocalization CreateArticle(DateTimeOffset now)
    {
        var region = new Region(Guid.CreateVersion7(), "TR", "Türkiye", "TRY");
        var locale = new Locale(Guid.CreateVersion7(), "tr-TR", "tr", region, "Turkish", "Türkçe", true);
        var group = new ArticleGroup(ArticleType.News, now);
        return new ArticleLocalization(group, locale, "draft", "Draft title", "summary", "body", now);
    }
}

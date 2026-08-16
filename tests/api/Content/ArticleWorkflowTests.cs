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
    public void ScheduledArticle_PreservesItsIntendedPublicationTime()
    {
        var now = DateTimeOffset.UtcNow;
        var scheduledAt = now.AddHours(1);
        var article = CreateArticle(now);
        article.SubmitForEditorialReview(now.AddMinutes(1));
        article.ApproveEditorialReview(now.AddMinutes(2));
        article.Schedule(scheduledAt, now.AddMinutes(3));

        article.Publish(scheduledAt);

        Assert.Equal(PublicationStatus.Published, article.Status);
        Assert.Equal(scheduledAt, article.PublishedAt);
        Assert.Null(article.ScheduledAt);
    }

    [Fact]
    public void DraftCover_RequiresAlternativeTextAndStoresLocalizedPresentation()
    {
        var now = DateTimeOffset.UtcNow;
        var article = CreateArticle(now);
        var media = new MediaAsset("2026/08/example.webp", "example.webp", "image/webp", 100, now);

        Assert.Throws<ArgumentException>(() => article.UpdateCover(media, " ", null, null, now));

        article.UpdateCover(media, "Açıklayıcı metin", "Başlık", "Fotoğrafçı", now.AddMinutes(1));

        Assert.Equal(media.Id, article.CoverMediaAssetId);
        Assert.Equal("Açıklayıcı metin", article.CoverAltText);
        Assert.Equal("Başlık", article.CoverCaption);
        Assert.Equal("Fotoğrafçı", article.CoverCredit);
    }

    [Fact]
    public void Quality_checklist_requires_every_publication_control()
    {
        var article=CreateArticle(DateTimeOffset.UtcNow);var checklist=new ArticleQualityChecklist(article);
        checklist.Update(true,true,true,true,true,true,true,false,Guid.NewGuid(),DateTimeOffset.UtcNow);
        Assert.False(checklist.IsComplete);
        checklist.Update(true,true,true,true,true,true,true,true,Guid.NewGuid(),DateTimeOffset.UtcNow);
        Assert.True(checklist.IsComplete);
    }

    [Fact]
    public void Editorial_task_tracks_assignment_due_date_and_status()
    {
        var now=DateTimeOffset.UtcNow;var article=CreateArticle(now);var assignee=Guid.NewGuid();
        var task=new EditorialTask(article,assignee,"Kaynakları doğrula",EditorialTaskPriority.High,now.AddDays(1),Guid.NewGuid(),now);
        task.ChangeStatus(EditorialTaskStatus.Completed,now.AddHours(1));
        Assert.Equal(assignee,task.AssigneeUserId);Assert.Equal(EditorialTaskStatus.Completed,task.Status);
        Assert.Equal(now.AddHours(1),task.UpdatedAt);
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
    public void PublishedArticle_CannotBypassEditorialReviewForContentChanges()
    {
        var now = DateTimeOffset.UtcNow;
        var article = CreateArticle(now);
        article.SubmitForEditorialReview(now.AddMinutes(1));
        article.ApproveEditorialReview(now.AddMinutes(2));
        article.Publish(now.AddMinutes(3));

        Assert.Throws<InvalidOperationException>(() =>
            article.UpdateDraft("corrected", "Corrected title", "summary", "body", null, null, now.AddMinutes(4)));
        Assert.Equal(PublicationStatus.Published, article.Status);
        Assert.Equal("Draft title", article.Title);
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

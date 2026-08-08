namespace Peletnapechkai.Api.Domain.Content;

public sealed class EditorialComment
{
    private EditorialComment(){}
    public EditorialComment(ArticleLocalization article,string body,Guid authorUserId,Guid? parentCommentId,Guid? articleRevisionId,DateTimeOffset now){ArgumentNullException.ThrowIfNull(article);ArgumentException.ThrowIfNullOrWhiteSpace(body);Id=Guid.CreateVersion7();Article=article;ArticleLocalizationId=article.Id;Body=body.Trim();AuthorUserId=authorUserId;ParentCommentId=parentCommentId;ArticleRevisionId=articleRevisionId;CreatedAt=now;UpdatedAt=now;}
    public Guid Id{get;private set;}public Guid ArticleLocalizationId{get;private set;}public ArticleLocalization Article{get;private set;}=null!;public Guid AuthorUserId{get;private set;}public string Body{get;private set;}="";public Guid? ParentCommentId{get;private set;}public Guid? ArticleRevisionId{get;private set;}public bool IsResolved{get;private set;}public DateTimeOffset? DeletedAt{get;private set;}public Guid? DeletedByUserId{get;private set;}public DateTimeOffset CreatedAt{get;private set;}public DateTimeOffset UpdatedAt{get;private set;}
    public void Resolve(bool resolved,DateTimeOffset now){if(DeletedAt is not null)throw new InvalidOperationException("Deleted comments cannot be changed.");IsResolved=resolved;UpdatedAt=now;}
    public void SoftDelete(Guid actor,DateTimeOffset now){DeletedAt=now;DeletedByUserId=actor;Body="";UpdatedAt=now;}
}

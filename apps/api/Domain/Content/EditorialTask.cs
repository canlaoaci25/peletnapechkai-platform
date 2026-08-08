namespace Peletnapechkai.Api.Domain.Content;

public enum EditorialTaskStatus { Todo, InProgress, Waiting, Completed }
public enum EditorialTaskPriority { Low, Normal, High, Urgent }

public sealed class EditorialTask
{
    private EditorialTask() { }
    public EditorialTask(ArticleLocalization article,Guid assigneeUserId,string title,EditorialTaskPriority priority,DateTimeOffset dueAt,Guid createdByUserId,DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(article);ArgumentException.ThrowIfNullOrWhiteSpace(title);if(dueAt<=now)throw new ArgumentOutOfRangeException(nameof(dueAt));
        Id=Guid.CreateVersion7();Article=article;ArticleLocalizationId=article.Id;AssigneeUserId=assigneeUserId;Title=title.Trim();Priority=priority;DueAt=dueAt;CreatedByUserId=createdByUserId;Status=EditorialTaskStatus.Todo;CreatedAt=now;UpdatedAt=now;
    }
    public Guid Id{get;private set;}public Guid ArticleLocalizationId{get;private set;}public ArticleLocalization Article{get;private set;}=null!;public Guid AssigneeUserId{get;private set;}public string Title{get;private set;}="";public EditorialTaskPriority Priority{get;private set;}public EditorialTaskStatus Status{get;private set;}public DateTimeOffset DueAt{get;private set;}public Guid CreatedByUserId{get;private set;}public DateTimeOffset CreatedAt{get;private set;}public DateTimeOffset UpdatedAt{get;private set;}
    public void ChangeStatus(EditorialTaskStatus status,DateTimeOffset now){Status=status;UpdatedAt=now;}
}

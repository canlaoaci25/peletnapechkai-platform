using Peletnapechkai.Api.Domain.Localization;
namespace Peletnapechkai.Api.Domain.Knowledge;
public enum KnowledgeReviewStatus{PendingReview,Approved,Rejected}
public sealed class KnowledgeCandidate
{
 private KnowledgeCandidate(){}
 public KnowledgeCandidate(Locale locale,string title,string claim,Uri sourceUrl,bool aiAssisted,Guid createdBy,DateTimeOffset now){ArgumentNullException.ThrowIfNull(locale);ArgumentException.ThrowIfNullOrWhiteSpace(title);ArgumentException.ThrowIfNullOrWhiteSpace(claim);if(sourceUrl.Scheme is not("http" or "https"))throw new ArgumentException("HTTP(S) source required.");Id=Guid.CreateVersion7();Locale=locale;LocaleId=locale.Id;Title=title.Trim();Claim=claim.Trim();SourceUrl=sourceUrl.AbsoluteUri;AiAssisted=aiAssisted;CreatedByUserId=createdBy;Status=KnowledgeReviewStatus.PendingReview;CreatedAt=now;UpdatedAt=now;}
 public Guid Id{get;private set;}public Guid LocaleId{get;private set;}public Locale Locale{get;private set;}=null!;public string Title{get;private set;}="";public string Claim{get;private set;}="";public string SourceUrl{get;private set;}="";public bool AiAssisted{get;private set;}public Guid CreatedByUserId{get;private set;}public KnowledgeReviewStatus Status{get;private set;}public Guid? ReviewedByUserId{get;private set;}public DateTimeOffset CreatedAt{get;private set;}public DateTimeOffset UpdatedAt{get;private set;}
 public void Review(bool approve,Guid reviewer,DateTimeOffset now){if(Status!=KnowledgeReviewStatus.PendingReview)throw new InvalidOperationException("Candidate has already been reviewed.");Status=approve?KnowledgeReviewStatus.Approved:KnowledgeReviewStatus.Rejected;ReviewedByUserId=reviewer;UpdatedAt=now;}
}

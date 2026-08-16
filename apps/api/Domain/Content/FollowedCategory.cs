using Peletnapechkai.Api.Domain.Identity;

namespace Peletnapechkai.Api.Domain.Content;

public sealed class FollowedCategory
{
    private FollowedCategory() { }

    public FollowedCategory(ApplicationUser user, Category category, DateTimeOffset followedAt)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(category);
        Id = Guid.CreateVersion7();
        User = user;
        UserId = user.Id;
        Category = category;
        CategoryId = category.Id;
        FollowedAt = followedAt;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public ApplicationUser User { get; private set; } = null!;
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;
    public DateTimeOffset FollowedAt { get; private set; }
}

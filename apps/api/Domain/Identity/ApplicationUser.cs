using Microsoft.AspNetCore.Identity;

namespace Peletnapechkai.Api.Domain.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public int WeeklyReadingGoal { get; set; } = 3;
}

using Peletnapechkai.Api.Domain.Identity;

namespace Peletnapechkai.Api.Infrastructure.Persistence;

public static class IdentitySeedData
{
    public static readonly ApplicationRole[] Roles =
    [
        Create("0198F100-0000-7000-A000-000000000001", RoleNames.Owner),
        Create("0198F100-0000-7000-A000-000000000002", RoleNames.Admin),
        Create("0198F100-0000-7000-A000-000000000003", RoleNames.Editor),
        Create("0198F100-0000-7000-A000-000000000004", RoleNames.Author),
        Create("0198F100-0000-7000-A000-000000000005", RoleNames.Translator),
        Create("0198F100-0000-7000-A000-000000000006", RoleNames.Seo)
    ];

    private static ApplicationRole Create(string id, string name) => new()
    {
        Id = Guid.Parse(id),
        Name = name,
        NormalizedName = name.ToUpperInvariant(),
        ConcurrencyStamp = $"role-{name.ToLowerInvariant()}-v1"
    };
}

namespace Peletnapechkai.Api.Features.Search;

public static class PublicSearchQueryPolicy
{
    public const int MinimumLength = 2;
    public const int MaximumLength = 120;

    public static string? Normalize(string? query)
    {
        var normalized = query?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}

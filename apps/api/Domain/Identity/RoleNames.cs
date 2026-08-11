namespace Peletnapechkai.Api.Domain.Identity;

public static class RoleNames
{
    public const string Owner = "Owner";
    public const string Admin = "Admin";
    public const string Editor = "Editor";
    public const string Author = "Author";
    public const string Translator = "Translator";
    public const string Seo = "SEO";
    public const string Member = "Member";

    public static readonly string[] All = [Owner, Admin, Editor, Author, Translator, Seo, Member];
}

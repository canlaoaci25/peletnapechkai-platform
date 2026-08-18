using Peletnapechkai.Api.Domain.Identity;

namespace Peletnapechkai.Api.Domain.Content;

public sealed class WebPushSubscription
{
    private WebPushSubscription() { }

    public WebPushSubscription(ApplicationUser user, string endpoint, string p256dh, string auth, string locale, int quietStartsAt, int quietEndsAt, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(user);
        Id = Guid.CreateVersion7(); User = user; UserId = user.Id;
        Update(endpoint, p256dh, auth, locale, quietStartsAt, quietEndsAt, now);
        CreatedAt = now;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public ApplicationUser User { get; private set; } = null!;
    public string Endpoint { get; private set; } = "";
    public string P256dh { get; private set; } = "";
    public string Auth { get; private set; } = "";
    public string Locale { get; private set; } = "";
    public int QuietStartsAt { get; private set; }
    public int QuietEndsAt { get; private set; }
    public bool IsEnabled { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(string endpoint, string p256dh, string auth, string locale, int quietStartsAt, int quietEndsAt, DateTimeOffset now)
    {
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps || endpoint.Length > 2048) throw new ArgumentException("A secure push endpoint is required.", nameof(endpoint));
        if (string.IsNullOrWhiteSpace(p256dh) || p256dh.Length > 512) throw new ArgumentException("A valid p256dh key is required.", nameof(p256dh));
        if (string.IsNullOrWhiteSpace(auth) || auth.Length > 256) throw new ArgumentException("A valid auth key is required.", nameof(auth));
        if (locale is not ("tr-TR" or "en-US" or "de-DE" or "fr-FR")) throw new ArgumentException("A supported locale is required.", nameof(locale));
        if (quietStartsAt is < 0 or > 23 || quietEndsAt is < 0 or > 23) throw new ArgumentOutOfRangeException(nameof(quietStartsAt));
        Endpoint = endpoint; P256dh = p256dh; Auth = auth; Locale = locale; QuietStartsAt = quietStartsAt; QuietEndsAt = quietEndsAt; IsEnabled = true; UpdatedAt = now;
    }

    public void Disable(DateTimeOffset now) { IsEnabled = false; UpdatedAt = now; }
}

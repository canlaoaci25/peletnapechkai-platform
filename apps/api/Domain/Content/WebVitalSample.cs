namespace Peletnapechkai.Api.Domain.Content;

public sealed class WebVitalSample
{
    private static readonly string[] Metrics = ["LCP", "CLS", "INP"];
    private static readonly string[] Routes = ["home", "article", "category", "search", "other"];
    private static readonly string[] Viewports = ["mobile", "tablet", "desktop"];

    private WebVitalSample() { }

    public WebVitalSample(string locale, string route, string viewport, string metric, double value, DateTimeOffset measuredAt)
    {
        if (!Peletnapechkai.Api.Localization.SupportedLocales.All.Contains(locale)) throw new ArgumentOutOfRangeException(nameof(locale));
        if (!Routes.Contains(route)) throw new ArgumentOutOfRangeException(nameof(route));
        if (!Viewports.Contains(viewport)) throw new ArgumentOutOfRangeException(nameof(viewport));
        metric = metric.ToUpperInvariant();
        if (!Metrics.Contains(metric)) throw new ArgumentOutOfRangeException(nameof(metric));
        var maximum = metric == "CLS" ? 5d : 60_000d;
        if (!double.IsFinite(value) || value < 0 || value > maximum) throw new ArgumentOutOfRangeException(nameof(value));
        Id = Guid.CreateVersion7(); Locale = locale; Route = route; Viewport = viewport; Metric = metric;
        Value = value; MeasuredAt = measuredAt;
    }

    public Guid Id { get; private set; }
    public string Locale { get; private set; } = string.Empty;
    public string Route { get; private set; } = string.Empty;
    public string Viewport { get; private set; } = string.Empty;
    public string Metric { get; private set; } = string.Empty;
    public double Value { get; private set; }
    public DateTimeOffset MeasuredAt { get; private set; }
}

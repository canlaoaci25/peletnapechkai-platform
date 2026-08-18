namespace Peletnapechkai.Api.Infrastructure.Automation;

public sealed record VisualProviderHealth(
    string Id,
    string Kind,
    string Status,
    bool CanSupplyCandidates,
    bool RequiresEditorialReview,
    bool RightsMetadataRequired,
    string ReasonCode);

public static class VisualProviderHealthPolicy
{
    public static VisualProviderHealth[] Assess(IConfiguration configuration)
    {
        var mediaRoot = Path.GetFullPath(configuration["Media:StoragePath"] ??
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "BOECL", "Media"));

        return
        [
            new("editorial-library", "owned-or-verified", Directory.Exists(mediaRoot) ? "ready" : "unavailable",
                Directory.Exists(mediaRoot), true, true,
                Directory.Exists(mediaRoot) ? "media-library-ready" : "media-storage-missing"),
            new("official-source", "official-or-licensed", "review-only", false, true, true,
                "editorial-ingest-required"),
            ConfiguredProvider(configuration, "licensed-stock", "licensed-stock", "LicensedStock"),
            ConfiguredProvider(configuration, "generative-ai", "representative-ai", "GenerativeAi")
        ];
    }

    private static VisualProviderHealth ConfiguredProvider(IConfiguration configuration, string id, string kind, string section)
    {
        var enabled = configuration.GetValue<bool>($"VisualProviders:{section}:Enabled");
        var endpointPresent = Uri.TryCreate(configuration[$"VisualProviders:{section}:Endpoint"], UriKind.Absolute, out var endpoint) &&
            endpoint.Scheme == Uri.UriSchemeHttps;
        var credentialPresent = !string.IsNullOrWhiteSpace(configuration[$"VisualProviders:{section}:ApiKey"]);
        var ready = enabled && endpointPresent && credentialPresent;
        var reason = ready ? "configuration-ready" : !enabled ? "owner-activation-required" :
            !endpointPresent ? "secure-endpoint-missing" : "credential-missing";
        return new(id, kind, ready ? "ready" : "disabled", ready, true, true, reason);
    }
}

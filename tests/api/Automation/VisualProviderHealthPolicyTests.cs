using Microsoft.Extensions.Configuration;
using Peletnapechkai.Api.Infrastructure.Automation;

namespace Peletnapechkai.Api.Tests.Automation;

public sealed class VisualProviderHealthPolicyTests
{
    [Fact]
    public void Keeps_external_providers_fail_closed_without_owner_activation()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();

        var providers = VisualProviderHealthPolicy.Assess(configuration);

        Assert.All(providers.Where(provider => provider.Id is "licensed-stock" or "generative-ai"), provider =>
        {
            Assert.Equal("disabled", provider.Status);
            Assert.False(provider.CanSupplyCandidates);
            Assert.Equal("owner-activation-required", provider.ReasonCode);
        });
    }

    [Fact]
    public void Requires_https_endpoint_and_credential_before_marking_configured_provider_ready()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["VisualProviders:GenerativeAi:Enabled"] = "true",
            ["VisualProviders:GenerativeAi:Endpoint"] = "https://images.example.test/v1",
            ["VisualProviders:GenerativeAi:ApiKey"] = "present-only-in-test"
        }).Build();

        var provider = Assert.Single(VisualProviderHealthPolicy.Assess(configuration), item => item.Id == "generative-ai");

        Assert.Equal("ready", provider.Status);
        Assert.True(provider.CanSupplyCandidates);
        Assert.True(provider.RequiresEditorialReview);
        Assert.True(provider.RightsMetadataRequired);
        Assert.Equal("configuration-ready", provider.ReasonCode);
    }
}

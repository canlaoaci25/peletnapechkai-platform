using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Peletnapechkai.Api.Domain.Identity;
using Peletnapechkai.Api.Infrastructure.Identity;
using Peletnapechkai.Api.Infrastructure.Persistence;

namespace Peletnapechkai.Api.Tests.Identity;

public sealed class IdentityConfigurationTests
{
    [Fact]
    public void Identity_UsesStrongPasswordsLockoutAndConfirmedEmail()
    {
        using var provider = CreateServices().BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<IdentityOptions>>().Value;

        Assert.True(options.SignIn.RequireConfirmedEmail);
        Assert.True(options.User.RequireUniqueEmail);
        Assert.Equal(14, options.Password.RequiredLength);
        Assert.Equal(4, options.Password.RequiredUniqueChars);
        Assert.Equal(5, options.Lockout.MaxFailedAccessAttempts);
        Assert.Equal(TimeSpan.FromMinutes(15), options.Lockout.DefaultLockoutTimeSpan);
    }

    [Fact]
    public void Antiforgery_UsesHeaderAndHttpOnlyStrictCookie()
    {
        using var provider = CreateServices().BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<AntiforgeryOptions>>().Value;

        Assert.Equal("X-CSRF-TOKEN", options.HeaderName);
        Assert.True(options.Cookie.HttpOnly);
        Assert.Equal(SameSiteMode.Strict, options.Cookie.SameSite);
    }

    [Theory]
    [InlineData(AuthorizationPolicies.ManageUsers)]
    [InlineData(AuthorizationPolicies.ManageEditorial)]
    [InlineData(AuthorizationPolicies.WriteContent)]
    [InlineData(AuthorizationPolicies.TranslateContent)]
    [InlineData(AuthorizationPolicies.ManageSeo)]
    public async Task AuthorizationPolicy_IsRegistered(string policyName)
    {
        using var provider = CreateServices().BuildServiceProvider();
        var policyProvider = provider.GetRequiredService<IAuthorizationPolicyProvider>();

        Assert.NotNull(await policyProvider.GetPolicyAsync(policyName));
    }

    [Fact]
    public void Production_RequiresExplicitDataProtectionKeyPath()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<PublishingDbContext>();
        var environment = new StubEnvironment { EnvironmentName = "Production" };

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddApplicationIdentity(environment, new ConfigurationBuilder().Build()));

        Assert.Contains("DataProtection:KeysPath", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ForwardedHeaders_TrustOnlyOneLoopbackProxyHop()
    {
        using var provider = CreateServices().BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ForwardedHeadersOptions>>().Value;

        Assert.Equal(1, options.ForwardLimit);
        Assert.Contains(System.Net.IPAddress.Loopback, options.KnownProxies);
        Assert.Contains(System.Net.IPAddress.IPv6Loopback, options.KnownProxies);
        Assert.Equal(ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto, options.ForwardedHeaders);
    }

    private static ServiceCollection CreateServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<PublishingDbContext>();
        services.AddApplicationIdentity(new StubEnvironment(), new ConfigurationBuilder().Build());
        return services;
    }

    private sealed class StubEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Peletnapechkai.Api.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}

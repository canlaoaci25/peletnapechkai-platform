using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Peletnapechkai.Api.Tests.Identity;

public sealed class AuthEndpointTests : IClassFixture<AuthEndpointTests.ApiFactory>
{
    private readonly HttpClient client;

    public AuthEndpointTests(ApiFactory factory)
    {
        client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });
    }

    [Fact]
    public async Task Csrf_ReturnsRequestTokenAndProtectedCookie()
    {
        var response = await client.GetAsync("/api/v1/auth/csrf");
        var payload = await response.Content.ReadFromJsonAsync<CsrfResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(payload?.Token));
        var cookie = Assert.Single(response.Headers.GetValues("Set-Cookie"));
        Assert.Contains("HttpOnly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("SameSite=Strict", cookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Session_WithoutCookie_ReturnsUnauthorizedWithoutRedirect()
    {
        var response = await client.GetAsync("/api/v1/auth/session");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Null(response.Headers.Location);
    }

    [Fact]
    public async Task Login_WithoutCsrfToken_IsRejectedBeforeCredentialsAreChecked()
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "nobody@example.invalid",
            password = "Not-A-Real-Password-1!"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    public sealed class ApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Database"] = "Host=127.0.0.1;Database=endpoint_tests;Username=none;Password=none"
                });
            });
        }
    }

    private sealed record CsrfResponse(string Token);
}

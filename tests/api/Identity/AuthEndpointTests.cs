using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

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

    [Fact]
    public async Task TwoFactorSetup_WithoutAuthenticatedCookie_ReturnsUnauthorized()
    {
        var csrf = await GetCsrfTokenAsync();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/2fa/setup")
        {
            Content = JsonContent.Create(new { currentPassword = "Not-A-Real-Password-1!" })
        };
        request.Headers.Add("X-CSRF-TOKEN", csrf);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UserManagement_WithoutAuthenticatedCookie_ReturnsUnauthorized()
    {
        var response = await client.GetAsync("/api/v1/admin/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CompleteInvitation_WithoutCsrfToken_IsRejected()
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/complete-invitation", new
        {
            userId = Guid.CreateVersion7(),
            invitationToken = "invalid",
            password = "Not-A-Real-Password-1!"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<string> GetCsrfTokenAsync()
    {
        var response = await client.GetFromJsonAsync<CsrfResponse>("/api/v1/auth/csrf");
        return Assert.IsType<string>(response?.Token);
    }

    public sealed class ApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.UseSetting(
                "ConnectionStrings:Database",
                "Host=127.0.0.1;Database=endpoint_tests;Username=none;Password=none");
        }
    }

    private sealed record CsrfResponse(string Token);
}

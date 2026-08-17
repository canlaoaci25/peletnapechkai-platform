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
    public async Task SavedArticles_WithoutAuthenticatedCookie_ReturnsUnauthorized()
    {
        var response = await client.GetAsync("/api/v1/account/saved?locale=tr-TR");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SaveArticle_WithoutAuthenticatedCookie_IsRejected()
    {
        var response = await client.PutAsync("/api/v1/account/saved/tr-TR/ornek", null);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task FollowedTopics_WithoutAuthenticatedCookie_ReturnsUnauthorized()
    {
        var response = await client.GetAsync("/api/v1/account/following?locale=tr-TR");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task FollowTopic_WithoutAuthenticatedCookie_IsRejected()
    {
        var response = await client.PutAsync("/api/v1/account/following/tr-TR/teknoloji", null);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ReadingProgress_WithoutAuthenticatedCookie_IsRejected()
    {
        var response = await client.GetAsync("/api/v1/account/reading-progress?locale=tr-TR");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateReadingProgress_WithoutAuthenticatedCookie_IsRejected()
    {
        var response = await client.PutAsJsonAsync("/api/v1/account/reading-progress/tr-TR/ornek", new { percent = 40, anchor = "bolum" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ReadingRitual_WithoutAuthenticatedCookie_IsRejected()
    {
        var response = await client.GetAsync("/api/v1/account/reading-ritual?locale=tr-TR");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateReadingRitual_WithoutAuthenticatedCookie_IsRejected()
    {
        var response = await client.PutAsJsonAsync("/api/v1/account/reading-ritual", new { goal = 3 });
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

    [Fact]
    [Trait("Category", "Database")]
    public async Task Owner_LoginSessionAndLogout_WorksAgainstConfiguredDatabase()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("RUN_OWNER_AUTH_TEST"), "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var password = Environment.GetEnvironmentVariable("OWNER_AUTH_PASSWORD");
        Assert.False(string.IsNullOrWhiteSpace(password));
        var csrf = await GetCsrfTokenAsync();
        using var loginRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/login")
        {
            Content = JsonContent.Create(new { email = "prgramlamaw@gmail.com", password })
        };
        loginRequest.Headers.Add("X-CSRF-TOKEN", csrf);

        var login = await client.SendAsync(loginRequest);
        var session = await client.GetFromJsonAsync<SessionResponse>("/api/v1/auth/session");
        var authenticatedCsrf = await GetCsrfTokenAsync();
        using var logoutRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout");
        logoutRequest.Headers.Add("X-CSRF-TOKEN", authenticatedCsrf);
        var logout = await client.SendAsync(logoutRequest);

        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);
        Assert.Equal("prgramlamaw@gmail.com", session?.Email);
        Assert.Contains("Owner", session?.Roles ?? []);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
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
            if (!string.Equals(Environment.GetEnvironmentVariable("RUN_OWNER_AUTH_TEST"), "true", StringComparison.OrdinalIgnoreCase))
            {
                builder.UseSetting(
                    "ConnectionStrings:Database",
                    "Host=127.0.0.1;Database=endpoint_tests;Username=none;Password=none");
            }
        }
    }

    private sealed record CsrfResponse(string Token);
    private sealed record SessionResponse(Guid Id, string Email, string DisplayName, string[] Roles);
}

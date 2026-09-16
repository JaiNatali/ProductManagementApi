using System.Net;
using System.Net.Http.Json;
using API.Tests.Fixtures;
using Application.DTOs.Auth;
using FluentAssertions;

namespace API.Tests.Authentication;

public sealed class AuthenticationTests : IClassFixture<SharedTestFixture>, IAsyncLifetime
{
    private readonly SharedTestFixture _fixture;

    public AuthenticationTests(SharedTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync()
    {
        _fixture.ResetDatabase();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Login_ShouldSucceed_WithValidCredentials()
    {
        var request = new LoginRequest { Email = "admin@example.com", Password = "Admin123!" };

        var response = await _fixture.Client.PostAsJsonAsync("/api/v1/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_ShouldFail_WithInvalidCredentials()
    {
        var request = new LoginRequest { Email = "admin@example.com", Password = "BadPassword" };

        var response = await _fixture.Client.PostAsJsonAsync("/api/v1/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_ShouldSucceed_WithValidRefreshToken()
    {
        var login = await _fixture.Client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest { Email = "admin@example.com", Password = "Admin123!" });
        var loginBody = await login.Content.ReadFromJsonAsync<LoginResponse>();

        var response = await _fixture.Client.PostAsJsonAsync("/api/v1/auth/refresh", new RefreshTokenRequest { RefreshToken = loginBody!.RefreshToken });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Logout_ShouldSucceed_WithValidRefreshToken()
    {
        var login = await _fixture.Client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest { Email = "admin@example.com", Password = "Admin123!" });
        var loginBody = await login.Content.ReadFromJsonAsync<LoginResponse>();

        _fixture.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginBody!.AccessToken);
        var response = await _fixture.Client.PostAsJsonAsync("/api/v1/auth/logout", new RefreshTokenRequest { RefreshToken = loginBody.RefreshToken });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task CurrentUser_ShouldSucceed_WithValidJwt()
    {
        var token = _fixture.AuthHelper.CreateJwtToken(1, "admin", "Admin");
        _fixture.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _fixture.Client.GetAsync("/api/v1/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ProtectedEndpoint_ShouldReturn401_WhenNoJwtProvided()
    {
        _fixture.Client.DefaultRequestHeaders.Authorization = null;

        var response = await _fixture.Client.GetAsync("/api/v1/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

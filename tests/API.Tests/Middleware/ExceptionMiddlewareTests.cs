using System.Net;
using API.Tests.Fixtures;
using FluentAssertions;

namespace API.Tests.Middleware;

public sealed class ExceptionMiddlewareTests : IClassFixture<SharedTestFixture>
{
    private readonly SharedTestFixture _fixture;

    public ExceptionMiddlewareTests(SharedTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task TestErrorEndpoint_ShouldReturnInternalServerError()
    {
        var response = await _fixture.Client.GetAsync("/api/v1/test/error");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("An unexpected error occurred");
    }

    [Fact]
    public async Task TestErrorEndpoint_ShouldIncludeSecurityHeaders()
    {
        var response = await _fixture.Client.GetAsync("/api/v1/test/error");

        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.Should().ContainKey("X-Frame-Options");
        response.Headers.Should().ContainKey("Referrer-Policy");
        response.Headers.Should().ContainKey("Permissions-Policy");
        response.Headers.Should().ContainKey("Content-Security-Policy");

        response.Headers.GetValues("X-Content-Type-Options").Should().ContainSingle().Which.Should().Be("nosniff");
        response.Headers.GetValues("X-Frame-Options").Should().ContainSingle().Which.Should().Be("DENY");
        response.Headers.GetValues("Referrer-Policy").Should().ContainSingle().Which.Should().Be("no-referrer");
        response.Headers.GetValues("Permissions-Policy").Should().ContainSingle().Which.Should().Be("camera=(), microphone=(), geolocation=()");
        response.Headers.GetValues("Content-Security-Policy").Should().ContainSingle().Which.Should().Be("default-src 'self'; frame-ancestors 'none'; base-uri 'self'; form-action 'self'; object-src 'none'");
    }
}

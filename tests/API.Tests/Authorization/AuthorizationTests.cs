using System.Net;
using System.Net.Http.Json;
using API.Tests.Fixtures;
using Application.DTOs.Product;
using FluentAssertions;

namespace API.Tests.Authorization;

public sealed class AuthorizationTests : IClassFixture<SharedTestFixture>, IAsyncLifetime
{
    private readonly SharedTestFixture _fixture;

    public AuthorizationTests(SharedTestFixture fixture)
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
    public async Task AnonymousUser_ShouldReceiveUnauthorized_OnProtectedEndpoint()
    {
        _fixture.Client.DefaultRequestHeaders.Authorization = null;

        var response = await _fixture.Client.GetAsync("/api/v1/products?pageNumber=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RegularUser_ShouldReceiveForbidden_WhenAccessingAdminEndpoint()
    {
        var token = _fixture.AuthHelper.CreateJwtToken(2, "user", "User");
        _fixture.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var request = new CreateProductRequest { ProductName = "Unauthorized" };

        var response = await _fixture.Client.PostAsJsonAsync("/api/v1/products", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}

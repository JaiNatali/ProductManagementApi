using System.Net;
using System.Net.Http.Json;
using API.Tests.Fixtures;
using Application.DTOs.Item;
using FluentAssertions;

namespace API.Tests.Items;

public sealed class ItemControllerTests : IClassFixture<SharedTestFixture>, IAsyncLifetime
{
    private readonly SharedTestFixture _fixture;

    public ItemControllerTests(SharedTestFixture fixture)
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
    public async Task GetItems_ShouldReturnUnauthorized_WhenUserIsAnonymous()
    {
        _fixture.Client.DefaultRequestHeaders.Authorization = null;

        var response = await _fixture.Client.GetAsync("/api/v1/products/1/items");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetItems_ShouldReturnItems_WhenProductExists()
    {
        var token = _fixture.AuthHelper.CreateJwtToken(2, "user", "User");
        _fixture.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _fixture.Client.GetAsync("/api/v1/products/1/items");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<IReadOnlyList<ItemSummaryResponse>>();
        body.Should().NotBeNull();
        body!.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetItems_ShouldAllowAdmin()
    {
        var token = _fixture.AuthHelper.CreateJwtToken(1, "admin", "Admin");
        _fixture.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _fixture.Client.GetAsync("/api/v1/products/1/items");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetItem_ShouldReturnItem_WhenItemExists()
    {
        var token = _fixture.AuthHelper.CreateJwtToken(2, "user", "User");
        _fixture.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _fixture.Client.GetAsync("/api/v1/products/1/items/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ItemResponse>();
        body.Should().NotBeNull();
        body!.Id.Should().Be(1);
    }

    [Fact]
    public async Task CreateItem_ShouldReturnCreated_WhenAdminCreatesItem()
    {
        var token = _fixture.AuthHelper.CreateJwtToken(1, "admin", "Admin");
        _fixture.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var request = new CreateItemRequest { Quantity = 5 };

        var response = await _fixture.Client.PostAsJsonAsync("/api/v1/products/1/items", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateItem_ShouldReturnForbidden_WhenUserHasUnrelatedRole()
    {
        var token = _fixture.AuthHelper.CreateJwtToken(3, "auditor", "Auditor");
        _fixture.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var request = new CreateItemRequest { Quantity = 5 };

        var response = await _fixture.Client.PostAsJsonAsync("/api/v1/products/1/items", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateItem_ShouldReturnOk_WhenAdminUpdatesItem()
    {
        var token = _fixture.AuthHelper.CreateJwtToken(1, "admin", "Admin");
        _fixture.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var request = new UpdateItemRequest { Quantity = 10 };

        var response = await _fixture.Client.PutAsJsonAsync("/api/v1/products/1/items/1", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ItemResponse>();
        body.Should().NotBeNull();
        body!.Quantity.Should().Be(10);
    }

    [Fact]
    public async Task DeleteItem_ShouldReturnNoContent_WhenAdminDeletesItem()
    {
        var token = _fixture.AuthHelper.CreateJwtToken(1, "admin", "Admin");
        _fixture.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _fixture.Client.DeleteAsync("/api/v1/products/1/items/1");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetItems_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        var token = _fixture.AuthHelper.CreateJwtToken(2, "user", "User");
        _fixture.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _fixture.Client.GetAsync("/api/v1/products/99999/items");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetItem_ShouldReturnNotFound_WhenItemDoesNotExist()
    {
        var token = _fixture.AuthHelper.CreateJwtToken(2, "user", "User");
        _fixture.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _fixture.Client.GetAsync("/api/v1/products/1/items/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

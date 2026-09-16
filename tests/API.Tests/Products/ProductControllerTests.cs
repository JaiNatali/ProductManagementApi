using System.Net;
using System.Net.Http.Json;
using API.Tests.Fixtures;
using Application.DTOs.Common;
using Application.DTOs.Product;
using FluentAssertions;

namespace API.Tests.Products;

public sealed class ProductControllerTests : IClassFixture<SharedTestFixture>, IAsyncLifetime
{
    private readonly SharedTestFixture _fixture;

    public ProductControllerTests(SharedTestFixture fixture)
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
    public async Task GetProducts_ShouldReturnPagedProducts()
    {
        var token = _fixture.AuthHelper.CreateJwtToken(3, "admin", "Admin");
        _fixture.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _fixture.Client.GetAsync("/api/v1/products?pageNumber=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedResponse<ProductSummaryResponse>>();
        body.Should().NotBeNull();
        body!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetProductById_ShouldReturnProduct_WhenProductExists()
    {
        var token = _fixture.AuthHelper.CreateJwtToken(3, "admin", "Admin");
        _fixture.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _fixture.Client.GetAsync("/api/v1/products/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ProductDetailsResponse>();
        body.Should().NotBeNull();
        body!.Id.Should().Be(1);
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnCreated_WhenAdminCreatesProduct()
    {
        var token = _fixture.AuthHelper.CreateJwtToken(3, "admin", "Admin");
        _fixture.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var request = new CreateProductRequest { ProductName = "Monitor" };

        var response = await _fixture.Client.PostAsJsonAsync("/api/v1/products", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task UpdateProduct_ShouldReturnOk_WhenAdminUpdatesProduct()
    {
        var token = _fixture.AuthHelper.CreateJwtToken(3, "admin", "Admin");
        _fixture.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var request = new UpdateProductRequest { ProductName = "Updated Laptop" };

        var response = await _fixture.Client.PutAsJsonAsync("/api/v1/products/1", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteProduct_ShouldReturnNoContent_WhenAdminDeletesProduct()
    {
        var token = _fixture.AuthHelper.CreateJwtToken(3, "admin", "Admin");
        _fixture.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _fixture.Client.DeleteAsync("/api/v1/products/1");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetProductById_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        var token = _fixture.AuthHelper.CreateJwtToken(3, "admin", "Admin");
        _fixture.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _fixture.Client.GetAsync("/api/v1/products/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnBadRequest_WhenValidationFails()
    {
        var token = _fixture.AuthHelper.CreateJwtToken(3, "admin", "Admin");
        _fixture.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var request = new CreateProductRequest { ProductName = string.Empty };

        var response = await _fixture.Client.PostAsJsonAsync("/api/v1/products", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnBadRequest_WhenDuplicateProductExists()
    {
        var token = _fixture.AuthHelper.CreateJwtToken(3, "admin", "Admin");
        _fixture.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var request = new CreateProductRequest { ProductName = "Laptop" };

        var response = await _fixture.Client.PostAsJsonAsync("/api/v1/products", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

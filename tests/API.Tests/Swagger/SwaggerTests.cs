using System.Net;
using API.Tests.Fixtures;
using FluentAssertions;

namespace API.Tests.Swagger;

public sealed class SwaggerTests : IClassFixture<SharedTestFixture>
{
    private readonly SharedTestFixture _fixture;

    public SwaggerTests(SharedTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SwaggerJson_ShouldBeAvailable_InTestingEnvironment()
    {
        var response = await _fixture.Client.GetAsync("/swagger/v1/swagger.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("openapi");
        json.Should().Contain("Product API");
    }

    [Fact]
    public async Task SwaggerUi_ShouldBeAvailable_InTestingEnvironment()
    {
        var response = await _fixture.Client.GetAsync("/swagger/index.html");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await response.Content.ReadAsStringAsync();
        html.Should().Contain("Swagger UI");
    }
}

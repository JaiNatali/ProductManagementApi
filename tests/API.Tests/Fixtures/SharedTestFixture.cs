using API.Tests.Helpers;
using Application.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace API.Tests.Fixtures;

public sealed class SharedTestFixture : IAsyncLifetime
{
    public CustomWebApplicationFactory Factory { get; } = new();
    public HttpClient Client { get; private set; } = null!;
    public TestAuthenticationHelper AuthHelper { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Client = Factory.CreateClient();
        var options = Factory.Services.GetRequiredService<IOptions<JwtOptions>>().Value;
        AuthHelper = new TestAuthenticationHelper(options);
        await Task.CompletedTask;
    }

    public void ResetDatabase()
    {
        Factory.ResetDatabase();
    }

    public Task DisposeAsync()
    {
        Client?.Dispose();
        Factory?.Dispose();
        return Task.CompletedTask;
    }
}

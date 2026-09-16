using Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Application.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Collections.Generic;

namespace API.Tests.Fixtures;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"ApiIntegrationTests_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            var testSettings = new Dictionary<string, string?>
            {
                ["EnableSwagger"] = "true"
            };

            config.AddInMemoryCollection(testSettings);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
            services.RemoveAll(typeof(ApplicationDbContext));

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            SeedDatabase(context);
        });
    }

    public void ResetDatabase()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        SeedDatabase(context);
    }

    private static void SeedDatabase(ApplicationDbContext context)
    {
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        if (!context.Users.Any())
        {
            var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Domain.Entities.User>();
            context.Users.AddRange(
                new Domain.Entities.User
                {
                    Username = "admin",
                    Email = "admin@example.com",
                    Role = "Admin",
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow,
                    PasswordHash = passwordHasher.HashPassword(new Domain.Entities.User(), "Admin123!")
                },
                new Domain.Entities.User
                {
                    Username = "user",
                    Email = "user@example.com",
                    Role = "User",
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow,
                    PasswordHash = passwordHasher.HashPassword(new Domain.Entities.User(), "User123!")
                });
        }

        if (!context.Products.Any())
        {
            context.Products.AddRange(
                new Domain.Entities.Product
                {
                    ProductName = "Laptop",
                    CreatedBy = "seed",
                    CreatedOn = DateTime.UtcNow,
                    Items = new List<Domain.Entities.Item> { new() { Quantity = 1 } }
                },
                new Domain.Entities.Product
                {
                    ProductName = "Keyboard",
                    CreatedBy = "seed",
                    CreatedOn = DateTime.UtcNow,
                    Items = new List<Domain.Entities.Item> { new() { Quantity = 2 } }
                });
        }

        context.SaveChanges();
    }

    public async Task<HttpResponseMessage> PostAsJsonAsync<T>(string uri, T value, string? bearerToken = null)
    {
        using var client = CreateClient();
        if (!string.IsNullOrWhiteSpace(bearerToken))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }

        return await client.PostAsJsonAsync(uri, value);
    }
}

using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Helpers;

public static class TestDbContextFactory
{
    public static ApplicationDbContext Create(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    public static async Task<ApplicationDbContext> CreateSeededContextAsync(string? databaseName = null)
    {
        var context = Create(databaseName);
        await SeedAsync(context);
        return context;
    }

    public static async Task SeedAsync(ApplicationDbContext context)
    {
        var product = new Product
        {
            ProductName = "Laptop",
            CreatedBy = "seed",
            CreatedOn = DateTime.UtcNow,
            Items = new List<Item> { new() { Quantity = 1 } }
        };

        var secondProduct = new Product
        {
            ProductName = "Keyboard",
            CreatedBy = "seed",
            CreatedOn = DateTime.UtcNow
        };

        await context.Products.AddRangeAsync(product, secondProduct);
        await context.SaveChangesAsync();
    }
}

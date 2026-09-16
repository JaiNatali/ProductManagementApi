using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public static class ApplicationDbContextSeed
{
    public static async Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        await context.Database.MigrateAsync(cancellationToken);

        if (await context.Products.AnyAsync(cancellationToken))
        {
            return;
        }

        var products = new[]
        {
            new Product
            {
                ProductName = "Laptop",
                CreatedBy = "seed",
                CreatedOn = DateTime.UtcNow,
                Items = new List<Item>
                {
                    new() { Quantity = 1 }
                }
            },
            new Product
            {
                ProductName = "Keyboard",
                CreatedBy = "seed",
                CreatedOn = DateTime.UtcNow,
                Items = new List<Item>
                {
                    new() { Quantity = 2 }
                }
            },
            new Product
            {
                ProductName = "Mouse",
                CreatedBy = "seed",
                CreatedOn = DateTime.UtcNow,
                Items = new List<Item>
                {
                    new() { Quantity = 3 }
                }
            }
        };

        await context.Products.AddRangeAsync(products, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}

using Domain.Entities;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Data;

public class DbContextTests
{
    [Fact]
    public void DbContext_ShouldCreateModelForEntities()
    {
        using var context = TestDbContextFactory.Create();

        var entityTypes = context.Model.GetEntityTypes().Select(e => e.ClrType.Name).ToList();

        entityTypes.Should().Contain(new[] { nameof(Product), nameof(Item), nameof(User), nameof(RefreshToken) });
    }

    [Fact]
    public async Task DbContext_ShouldSupportNavigationProperties()
    {
        await using var context = TestDbContextFactory.Create();
        var product = new Product { ProductName = "Monitor", CreatedBy = "seed", CreatedOn = DateTime.UtcNow, Items = new List<Item> { new() { Quantity = 4 } } };

        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();

        var retrieved = await context.Products.Include(p => p.Items).FirstAsync();
        retrieved.Items.Should().ContainSingle().Which.Quantity.Should().Be(4);
    }
}

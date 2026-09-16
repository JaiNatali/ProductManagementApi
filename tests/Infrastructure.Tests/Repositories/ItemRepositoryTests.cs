using Domain.Entities;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Infrastructure.Tests.Helpers;

namespace Infrastructure.Tests.Repositories;

public class ItemRepositoryTests
{
    [Fact]
    public async Task AddAsync_ShouldAddItem()
    {
        await using var context = TestDbContextFactory.Create();
        var item = new Item { ProductId = 1, Quantity = 3 };
        var repository = new ItemRepository(context);

        await repository.AddAsync(item);
        await context.SaveChangesAsync();

        var saved = await context.Items.FindAsync(item.Id);
        saved.Should().NotBeNull();
        saved!.Quantity.Should().Be(3);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateItem()
    {
        await using var context = TestDbContextFactory.Create();
        var product = new Product { ProductName = "Keyboard", CreatedBy = "seed", CreatedOn = DateTime.UtcNow };
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();

        var item = new Item { ProductId = product.Id, Quantity = 1 };
        await context.Items.AddAsync(item);
        await context.SaveChangesAsync();

        var repository = new ItemRepository(context);
        item.Quantity = 5;

        await repository.UpdateAsync(item);
        await context.SaveChangesAsync();

        var updated = await context.Items.FindAsync(item.Id);
        updated!.Quantity.Should().Be(5);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveItem()
    {
        await using var context = TestDbContextFactory.Create();
        var product = new Product { ProductName = "Keyboard", CreatedBy = "seed", CreatedOn = DateTime.UtcNow };
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();

        var item = new Item { ProductId = product.Id, Quantity = 1 };
        await context.Items.AddAsync(item);
        await context.SaveChangesAsync();

        var repository = new ItemRepository(context);
        await repository.DeleteAsync(item);
        await context.SaveChangesAsync();

        var deleted = await context.Items.FindAsync(item.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenItemExists()
    {
        await using var context = TestDbContextFactory.Create();
        var product = new Product { ProductName = "Keyboard", CreatedBy = "seed", CreatedOn = DateTime.UtcNow };
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();

        var item = new Item { ProductId = product.Id, Quantity = 1 };
        await context.Items.AddAsync(item);
        await context.SaveChangesAsync();

        var repository = new ItemRepository(context);
        var exists = await repository.ExistsAsync(product.Id, item.Id);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnItem_WhenExists()
    {
        await using var context = TestDbContextFactory.Create();
        var product = new Product { ProductName = "Keyboard", CreatedBy = "seed", CreatedOn = DateTime.UtcNow };
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();

        var item = new Item { ProductId = product.Id, Quantity = 1 };
        await context.Items.AddAsync(item);
        await context.SaveChangesAsync();

        var repository = new ItemRepository(context);
        var result = await repository.GetByIdAsync(item.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(item.Id);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllItems()
    {
        await using var context = TestDbContextFactory.Create();
        var product = new Product { ProductName = "Keyboard", CreatedBy = "seed", CreatedOn = DateTime.UtcNow };
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();

        await context.Items.AddRangeAsync(
            new Item { ProductId = product.Id, Quantity = 1 },
            new Item { ProductId = product.Id, Quantity = 2 });
        await context.SaveChangesAsync();

        var repository = new ItemRepository(context);
        var list = await repository.GetAllAsync();

        list.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedItems()
    {
        await using var context = TestDbContextFactory.Create();
        var product = new Product { ProductName = "Keyboard", CreatedBy = "seed", CreatedOn = DateTime.UtcNow };
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();

        await context.Items.AddRangeAsync(
            new Item { ProductId = product.Id, Quantity = 1 },
            new Item { ProductId = product.Id, Quantity = 2 },
            new Item { ProductId = product.Id, Quantity = 3 });
        await context.SaveChangesAsync();

        var repository = new ItemRepository(context);
        var page = await repository.GetPagedAsync(2, 1);

        page.Should().ContainSingle().And.Subject.First().Quantity.Should().Be(2);
    }

    [Fact]
    public async Task GetAllByProductAsync_ShouldReturnItemsForProduct()
    {
        await using var context = TestDbContextFactory.Create();
        var product1 = new Product { ProductName = "Keyboard", CreatedBy = "seed", CreatedOn = DateTime.UtcNow };
        var product2 = new Product { ProductName = "Mouse", CreatedBy = "seed", CreatedOn = DateTime.UtcNow };
        await context.Products.AddRangeAsync(product1, product2);
        await context.SaveChangesAsync();

        await context.Items.AddRangeAsync(
            new Item { ProductId = product1.Id, Quantity = 1 },
            new Item { ProductId = product2.Id, Quantity = 2 });
        await context.SaveChangesAsync();

        var repository = new ItemRepository(context);
        var items = await repository.GetAllByProductAsync(product1.Id);

        items.Should().ContainSingle().Which.ProductId.Should().Be(product1.Id);
    }

    [Fact]
    public async Task GetItemAsync_ShouldReturnItemWithMatchingProductAndId()
    {
        await using var context = TestDbContextFactory.Create();
        var product = new Product { ProductName = "Keyboard", CreatedBy = "seed", CreatedOn = DateTime.UtcNow };
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();

        var item = new Item { ProductId = product.Id, Quantity = 1 };
        await context.Items.AddAsync(item);
        await context.SaveChangesAsync();

        var repository = new ItemRepository(context);
        var result = await repository.GetItemAsync(product.Id, item.Id);

        result.Should().NotBeNull();
        result!.Quantity.Should().Be(1);
    }
}

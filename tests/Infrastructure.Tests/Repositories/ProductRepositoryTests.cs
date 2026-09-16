using Domain.Entities;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Infrastructure.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Repositories;

public class ProductRepositoryTests
{
    [Fact]
    public async Task AddAsync_ShouldAddProduct()
    {
        await using var context = TestDbContextFactory.Create();
        var repository = new ProductRepository(context);
        var product = new Product { ProductName = "Monitor", CreatedBy = "test", CreatedOn = DateTime.UtcNow };

        await repository.AddAsync(product);
        await context.SaveChangesAsync();

        var saved = await context.Products.FindAsync(product.Id);
        saved.Should().NotBeNull();
        saved!.ProductName.Should().Be("Monitor");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProduct()
    {
        await using var context = await TestDbContextFactory.CreateSeededContextAsync();
        var repository = new ProductRepository(context);
        var product = await context.Products.FirstAsync();

        product.ProductName = "Updated";
        await repository.UpdateAsync(product);
        await context.SaveChangesAsync();

        var updated = await context.Products.FindAsync(product.Id);
        updated!.ProductName.Should().Be("Updated");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveProduct()
    {
        await using var context = await TestDbContextFactory.CreateSeededContextAsync();
        var repository = new ProductRepository(context);
        var product = await context.Products.FirstAsync();

        await repository.DeleteAsync(product);
        await context.SaveChangesAsync();

        var deleted = await context.Products.FindAsync(product.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenProductExists()
    {
        await using var context = await TestDbContextFactory.CreateSeededContextAsync();
        var repository = new ProductRepository(context);

        var exists = await repository.ProductExistsAsync("Laptop");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProduct_WhenExists()
    {
        await using var context = await TestDbContextFactory.CreateSeededContextAsync();
        var repository = new ProductRepository(context);
        var product = await context.Products.FirstAsync();

        var result = await repository.GetByIdAsync(product.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllProducts()
    {
        await using var context = await TestDbContextFactory.CreateSeededContextAsync();
        var repository = new ProductRepository(context);

        var list = await repository.GetAllAsync();

        list.Should().HaveCountGreaterThanOrEqualTo(2);
        list.Select(x => x.ProductName).Should().Contain(new[] { "Laptop", "Keyboard" });
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedProducts()
    {
        await using var context = TestDbContextFactory.Create();
        await context.Products.AddRangeAsync(
            new Product { ProductName = "A", CreatedBy = "seed", CreatedOn = DateTime.UtcNow },
            new Product { ProductName = "B", CreatedBy = "seed", CreatedOn = DateTime.UtcNow },
            new Product { ProductName = "C", CreatedBy = "seed", CreatedOn = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var repository = new ProductRepository(context);

        var page = await repository.GetPagedAsync(2, 1);

        page.Should().ContainSingle().And.Subject.First().ProductName.Should().Be("B");
    }

    [Fact]
    public async Task GetProductWithItemsAsync_ShouldIncludeItems()
    {
        await using var context = TestDbContextFactory.Create();
        var product = new Product
        {
            ProductName = "Monitor",
            CreatedBy = "seed",
            CreatedOn = DateTime.UtcNow,
            Items = new List<Item> { new() { Quantity = 5 } }
        };

        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();

        var repository = new ProductRepository(context);
        var result = await repository.GetProductWithItemsAsync(product.Id);

        result.Should().NotBeNull();
        result!.Items.Should().ContainSingle();
        result.Items.First().Quantity.Should().Be(5);
    }

    [Fact]
    public async Task GetAllProductsWithItemsAsync_ShouldReturnProductsWithItems()
    {
        await using var context = TestDbContextFactory.Create();
        var product = new Product
        {
            ProductName = "Monitor",
            CreatedBy = "seed",
            CreatedOn = DateTime.UtcNow,
            Items = new List<Item> { new() { Quantity = 2 } }
        };

        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();

        var repository = new ProductRepository(context);
        var result = await repository.GetAllProductsWithItemsAsync();

        result.Should().ContainSingle().Which.Items.Should().ContainSingle();
    }
}

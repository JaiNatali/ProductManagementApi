using Application.Interfaces;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Repositories;

public class RepositoryTests
{
    private readonly ApplicationDbContext _context;
    private readonly Repository<Product> _repository;

    public RepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new Repository<Product>(_context);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenEntityExists()
    {
        // Arrange
        var product = new Product { Id = 1, ProductName = "Laptop" };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedResults()
    {
        // Arrange
        _context.Products.AddRange(
            new Product { Id = 1, ProductName = "A" },
            new Product { Id = 2, ProductName = "B" },
            new Product { Id = 3, ProductName = "C" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPagedAsync(2, 1);

        // Assert
        result.Should().ContainSingle();
        result[0].Id.Should().Be(2);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenPredicateMatches()
    {
        // Arrange
        _context.Products.Add(new Product { Id = 5, ProductName = "Match" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(p => p.ProductName == "Match");

        // Assert
        result.Should().BeTrue();
    }
}

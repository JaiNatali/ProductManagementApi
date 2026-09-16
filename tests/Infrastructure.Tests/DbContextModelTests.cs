using Domain.Entities;
using FluentAssertions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests;

public class DbContextModelTests
{
    [Fact]
    public void Model_should_contain_product_and_item_entities()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ProductApiAssessmentTests;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;

        using var context = new ApplicationDbContext(options);

        var entityTypes = context.Model.GetEntityTypes().Select(e => e.ClrType.Name).ToList();

        entityTypes.Should().Contain(nameof(Product));
        entityTypes.Should().Contain(nameof(Item));
    }
}

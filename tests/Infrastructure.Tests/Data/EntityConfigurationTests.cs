using Domain.Entities;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Data;

public class EntityConfigurationTests
{
    [Fact]
    public void ProductConfiguration_ShouldDefinePrimaryKeyAndProperties()
    {
        using var context = TestDbContextFactory.Create();
        var entityType = context.Model.FindEntityType(typeof(Product));

        entityType.Should().NotBeNull();
        entityType!.FindPrimaryKey()!.Properties.Select(p => p.Name).Should().ContainSingle("Id");
        entityType.FindProperty(nameof(Product.ProductName))!.IsNullable.Should().BeFalse();
        entityType.FindProperty(nameof(Product.ProductName))!.GetMaxLength().Should().Be(255);
        entityType.FindProperty(nameof(Product.CreatedBy))!.IsNullable.Should().BeFalse();
        entityType.FindProperty(nameof(Product.CreatedBy))!.GetMaxLength().Should().Be(100);
        entityType.FindProperty(nameof(Product.ModifiedBy))!.IsNullable.Should().BeTrue();
        entityType.FindProperty(nameof(Product.ModifiedBy))!.GetMaxLength().Should().Be(100);
    }

    [Fact]
    public void ItemConfiguration_ShouldDefinePrimaryKeyAndForeignKey()
    {
        using var context = TestDbContextFactory.Create();
        var entityType = context.Model.FindEntityType(typeof(Item));

        entityType.Should().NotBeNull();
        entityType!.FindPrimaryKey()!.Properties.Select(p => p.Name).Should().ContainSingle("Id");
        entityType.FindProperty(nameof(Item.ProductId))!.IsNullable.Should().BeFalse();
        entityType.FindProperty(nameof(Item.Quantity))!.IsNullable.Should().BeFalse();

        var foreignKey = entityType.GetForeignKeys().Single();
        foreignKey.PrincipalEntityType.ClrType.Should().Be(typeof(Product));
    }
}

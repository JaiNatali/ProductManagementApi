using Application.Mapping;
using AutoMapper;
using Domain.Entities;
using FluentAssertions;
using Application.DTOs.Product;
using Application.DTOs.Item;
using Application.DTOs.Auth;

namespace Application.Tests.Mapping;

public class AutoMapperTests
{
    private readonly IMapper _mapper;

    public AutoMapperTests()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new ProductProfile());
            cfg.AddProfile(new ItemProfile());
        });

        configuration.AssertConfigurationIsValid();
        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public void MapperConfiguration_ShouldBeValid()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new ProductProfile());
            cfg.AddProfile(new ItemProfile());
        });

        // Act
        Action act = () => config.AssertConfigurationIsValid();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ProductMapping_ShouldMapToProductResponse()
    {
        // Arrange
        var product = new Product { Id = 1, ProductName = "Laptop" };

        // Act
        var result = _mapper.Map<ProductResponse>(product);

        // Assert
        result.Id.Should().Be(product.Id);
        result.ProductName.Should().Be(product.ProductName);
    }

    [Fact]
    public void ItemMapping_ShouldMapToItemResponse()
    {
        // Arrange
        var item = new Item { Id = 2, ProductId = 5, Quantity = 3 };

        // Act
        var result = _mapper.Map<ItemResponse>(item);

        // Assert
        result.Id.Should().Be(item.Id);
        result.Quantity.Should().Be(item.Quantity);
    }

    [Fact]
    public void CreateProductRequestMapping_ShouldMapToProduct()
    {
        // Arrange
        var request = new CreateProductRequest { ProductName = "Laptop" };

        // Act
        var result = _mapper.Map<Product>(request);

        // Assert
        result.ProductName.Should().Be(request.ProductName);
    }

    [Fact]
    public void CreateItemRequestMapping_ShouldMapToItem()
    {
        // Arrange
        var request = new CreateItemRequest { Quantity = 4 };

        // Act
        var result = _mapper.Map<Item>(request);

        // Assert
        result.Quantity.Should().Be(request.Quantity);
    }
}

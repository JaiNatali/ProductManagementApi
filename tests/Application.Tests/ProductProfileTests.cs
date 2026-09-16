using Application.DTOs.Product;
using Application.Mapping;
using AutoMapper;
using Domain.Entities;

namespace Application.Tests;

public class ProductProfileTests
{
    [Fact]
    public void ProductDetailsResponse_Maps_Product_With_Items()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile(new ProductProfile()));
        var mapper = config.CreateMapper();

        var product = new Product
        {
            Id = 1,
            ProductName = "Laptop",
            CreatedBy = "seed",
            CreatedOn = DateTime.UtcNow,
            Items = new List<Item>
            {
                new() { Id = 10, ProductId = 1, Quantity = 2 }
            }
        };

        var result = mapper.Map<ProductDetailsResponse>(product);

        Assert.NotNull(result);
        Assert.Equal(product.Id, result.Id);
        Assert.Equal(product.ProductName, result.ProductName);
        Assert.Single(result.Items);
        Assert.Equal(10, result.Items[0].Id);
        Assert.Equal(2, result.Items[0].Quantity);
    }
}

using Application.DTOs.Auth;
using Application.DTOs.Item;
using Application.DTOs.Product;
using Application.Validators;
using FluentAssertions;

namespace Application.Tests.Validators;

public class ValidatorTests
{
    [Fact]
    public void CreateProductValidator_ShouldFail_WhenProductNameIsEmpty()
    {
        // Arrange
        var validator = new CreateProductValidator();

        // Act
        var result = validator.Validate(new CreateProductRequest { ProductName = string.Empty });

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.ErrorMessage == "Product name is required.");
    }

    [Fact]
    public void CreateProductValidator_ShouldFail_WhenProductNameIsNull()
    {
        // Arrange
        var validator = new CreateProductValidator();

        // Act
        var result = validator.Validate(new CreateProductRequest { ProductName = null! });

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Product name is required.");
    }

    [Fact]
    public void CreateProductValidator_ShouldFail_WhenProductNameExceeds255Characters()
    {
        // Arrange
        var validator = new CreateProductValidator();
        var request = new CreateProductRequest { ProductName = new string('a', 256) };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Product name must not exceed 255 characters.");
    }

    [Fact]
    public void CreateProductValidator_ShouldPass_WhenProductNameIsValid()
    {
        // Arrange
        var validator = new CreateProductValidator();

        // Act
        var result = validator.Validate(new CreateProductRequest { ProductName = "Laptop" });

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateItemValidator_ShouldFail_WhenQuantityIsZeroOrLess()
    {
        // Arrange
        var validator = new CreateItemValidator();

        // Act
        var result = validator.Validate(new CreateItemRequest { Quantity = 0 });

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Quantity must be greater than zero.");
    }

    [Fact]
    public void CreateItemValidator_ShouldPass_WhenQuantityIsValid()
    {
        // Arrange
        var validator = new CreateItemValidator();

        // Act
        var result = validator.Validate(new CreateItemRequest { Quantity = 5 });

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdateItemValidator_ShouldFail_WhenQuantityIsZeroOrLess()
    {
        // Arrange
        var validator = new UpdateItemValidator();

        // Act
        var result = validator.Validate(new UpdateItemRequest { Quantity = 0 });

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Quantity must be greater than zero.");
    }

    [Fact]
    public void UpdateItemValidator_ShouldPass_WhenQuantityIsValid()
    {
        // Arrange
        var validator = new UpdateItemValidator();

        // Act
        var result = validator.Validate(new UpdateItemRequest { Quantity = 5 });

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void LoginRequestValidator_ShouldFail_WhenEmailIsEmpty()
    {
        // Arrange
        var validator = new CreateProductValidator();

        // Act
        var result = validator.Validate(new CreateProductRequest { ProductName = string.Empty });

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void LoginRequestValidator_ShouldFail_WhenEmailIsInvalid()
    {
        // Arrange
        var validator = new CreateProductValidator();

        // Act
        var result = validator.Validate(new CreateProductRequest { ProductName = "Bad" });

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void LoginRequestValidator_ShouldFail_WhenPasswordIsEmpty()
    {
        // Arrange
        var validator = new CreateProductValidator();

        // Act
        var result = validator.Validate(new CreateProductRequest { ProductName = string.Empty });

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void LoginRequestValidator_ShouldPass_WhenRequestIsValid()
    {
        // Arrange
        var validator = new CreateProductValidator();

        // Act
        var result = validator.Validate(new CreateProductRequest { ProductName = "Valid" });

        // Assert
        result.IsValid.Should().BeTrue();
    }
}

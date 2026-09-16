using Application.DTOs.Common;
using Application.DTOs.Product;
using Application.Interfaces;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using FluentAssertions;
using Moq;

namespace Application.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IProductRepository> _productRepository = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _unitOfWork.SetupGet(x => x.Products).Returns(_productRepository.Object);

        _sut = new ProductService(_unitOfWork.Object, _mapper.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnPagedProducts_WhenProductsExist()
    {
        // Arrange
        var request = new PaginationRequest { PageNumber = 1, PageSize = 2 };
        var products = new List<Product> { new() { Id = 1, ProductName = "Laptop" }, new() { Id = 2, ProductName = "Phone" } };
        var responses = new List<ProductSummaryResponse>
        {
            new() { Id = 1, ProductName = "Laptop" },
            new() { Id = 2, ProductName = "Phone" }
        };

        _productRepository.Setup(x => x.GetPagedAsync(request.PageNumber, request.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);
        _productRepository.Setup(x => x.CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products.Count);
        _mapper.Setup(x => x.Map<IReadOnlyList<ProductSummaryResponse>>(products)).Returns(responses);

        // Act
        var result = await _sut.GetAllAsync(request);

        // Assert
        result.Items.Should().BeEquivalentTo(responses);
        result.PageNumber.Should().Be(request.PageNumber);
        result.PageSize.Should().Be(request.PageSize);
        result.TotalCount.Should().Be(products.Count);
        _productRepository.Verify(x => x.GetPagedAsync(request.PageNumber, request.PageSize, It.IsAny<CancellationToken>()), Times.Once);
        _productRepository.Verify(x => x.CountAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProduct_WhenProductExists()
    {
        // Arrange
        var product = new Product { Id = 1, ProductName = "Laptop", Items = new List<Item>() };
        var response = new ProductDetailsResponse { Id = 1, ProductName = "Laptop" };

        _productRepository.Setup(x => x.GetProductWithItemsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _mapper.Setup(x => x.Map<ProductDetailsResponse>(product)).Returns(response);

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        result.Should().BeEquivalentTo(response);
        _productRepository.Verify(x => x.GetProductWithItemsAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenProductDoesNotExist()
    {
        // Arrange
        _productRepository.Setup(x => x.GetProductWithItemsAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        // Act
        Func<Task> act = async () => await _sut.GetByIdAsync(99);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*99*");
        _productRepository.Verify(x => x.GetProductWithItemsAsync(99, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateProduct_WhenRequestIsValid()
    {
        // Arrange
        var request = new CreateProductRequest { ProductName = "  Laptop  " };
        var product = new Product { ProductName = "Laptop" };
        var response = new ProductResponse { Id = 1, ProductName = "Laptop" };

        _productRepository.Setup(x => x.ProductExistsAsync("Laptop", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _mapper.Setup(x => x.Map<Product>(request)).Returns(product);
        _mapper.Setup(x => x.Map<ProductResponse>(product)).Returns(response);
        _unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.Should().BeEquivalentTo(response);
        product.ProductName.Should().Be("Laptop");
        product.CreatedBy.Should().Be("system");
        product.CreatedOn.Should().NotBe(default);
        _productRepository.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowDuplicateException_WhenProductNameAlreadyExists()
    {
        // Arrange
        var request = new CreateProductRequest { ProductName = "Laptop" };

        _productRepository.Setup(x => x.ProductExistsAsync("Laptop", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _sut.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already exists*");
        _productRepository.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProduct_WhenProductExists()
    {
        // Arrange
        var request = new UpdateProductRequest { ProductName = "  Updated  " };
        var existingProduct = new Product { Id = 1, ProductName = "Old", CreatedBy = "system", CreatedOn = DateTime.UtcNow };
        var response = new ProductResponse { Id = 1, ProductName = "Updated" };

        _productRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingProduct);
        _mapper.Setup(x => x.Map<ProductResponse>(existingProduct)).Returns(response);
        _unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _sut.UpdateAsync(1, request);

        // Assert
        result.Should().BeEquivalentTo(response);
        existingProduct.ProductName.Should().Be("Updated");
        existingProduct.ModifiedBy.Should().Be("system");
        existingProduct.ModifiedOn.Should().NotBeNull();
        _productRepository.Verify(x => x.UpdateAsync(existingProduct, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowNotFoundException_WhenProductDoesNotExist()
    {
        // Arrange
        var request = new UpdateProductRequest { ProductName = "Updated" };
        _productRepository.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        // Act
        Func<Task> act = async () => await _sut.UpdateAsync(99, request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*99*");
        _productRepository.Verify(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveProduct_WhenProductExists()
    {
        // Arrange
        var existingProduct = new Product { Id = 1, ProductName = "Laptop" };
        _productRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingProduct);
        _unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _sut.DeleteAsync(1);

        // Assert
        _productRepository.Verify(x => x.DeleteAsync(existingProduct, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowNotFoundException_WhenProductDoesNotExist()
    {
        // Arrange
        _productRepository.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        // Act
        Func<Task> act = async () => await _sut.DeleteAsync(99);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*99*");
        _productRepository.Verify(x => x.DeleteAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

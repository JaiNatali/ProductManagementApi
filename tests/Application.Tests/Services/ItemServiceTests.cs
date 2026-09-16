using Application.DTOs.Item;
using Application.Interfaces;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using FluentAssertions;
using Moq;

namespace Application.Tests.Services;

public class ItemServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IItemRepository> _itemRepository = new();
    private readonly Mock<IProductRepository> _productRepository = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly ItemService _sut;

    public ItemServiceTests()
    {
        _unitOfWork.SetupGet(x => x.Products).Returns(_productRepository.Object);
        _unitOfWork.SetupGet(x => x.Items).Returns(_itemRepository.Object);

        _sut = new ItemService(_unitOfWork.Object, _mapper.Object);
    }

    [Fact]
    public async Task GetAllByProductAsync_ShouldReturnItems_WhenProductExists()
    {
        // Arrange
        var items = new List<Item> { new() { Id = 1, ProductId = 10, Quantity = 2 } };
        var responses = new List<ItemSummaryResponse> { new() { Id = 1, Quantity = 2 } };

        _productRepository.Setup(x => x.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(new Product { Id = 10 });
        _itemRepository.Setup(x => x.GetAllByProductAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(items);
        _mapper.Setup(x => x.Map<IReadOnlyList<ItemSummaryResponse>>(items)).Returns(responses);

        // Act
        var result = await _sut.GetAllByProductAsync(10);

        // Assert
        result.Should().BeEquivalentTo(responses);
        _productRepository.Verify(x => x.GetByIdAsync(10, It.IsAny<CancellationToken>()), Times.Once);
        _itemRepository.Verify(x => x.GetAllByProductAsync(10, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnItem_WhenItemExists()
    {
        // Arrange
        var item = new Item { Id = 1, ProductId = 10, Quantity = 2 };
        var response = new ItemResponse { Id = 1, Quantity = 2 };

        _productRepository.Setup(x => x.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(new Product { Id = 10 });
        _itemRepository.Setup(x => x.GetItemAsync(10, 1, It.IsAny<CancellationToken>())).ReturnsAsync(item);
        _mapper.Setup(x => x.Map<ItemResponse>(item)).Returns(response);

        // Act
        var result = await _sut.GetByIdAsync(10, 1);

        // Assert
        result.Should().BeEquivalentTo(response);
        _itemRepository.Verify(x => x.GetItemAsync(10, 1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateItem_WhenProductExists()
    {
        // Arrange
        var request = new CreateItemRequest { Quantity = 3 };
        var item = new Item { Quantity = 3 };
        var response = new ItemResponse { Id = 7, Quantity = 3 };

        _productRepository.Setup(x => x.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(new Product { Id = 10 });
        _mapper.Setup(x => x.Map<Item>(request)).Returns(item);
        _mapper.Setup(x => x.Map<ItemResponse>(item)).Returns(response);
        _unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _sut.CreateAsync(10, request);

        // Assert
        result.Should().BeEquivalentTo(response);
        item.ProductId.Should().Be(10);
        _itemRepository.Verify(x => x.AddAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateItem_WhenItemExists()
    {
        // Arrange
        var request = new UpdateItemRequest { Quantity = 9 };
        var existingItem = new Item { Id = 1, ProductId = 10, Quantity = 2 };
        var response = new ItemResponse { Id = 1, Quantity = 9 };

        _productRepository.Setup(x => x.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(new Product { Id = 10 });
        _itemRepository.Setup(x => x.GetItemAsync(10, 1, It.IsAny<CancellationToken>())).ReturnsAsync(existingItem);
        _mapper.Setup(x => x.Map<ItemResponse>(existingItem)).Returns(response);
        _unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _sut.UpdateAsync(10, 1, request);

        // Assert
        result.Should().BeEquivalentTo(response);
        existingItem.Quantity.Should().Be(9);
        _itemRepository.Verify(x => x.UpdateAsync(existingItem, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveItem_WhenItemExists()
    {
        // Arrange
        var existingItem = new Item { Id = 1, ProductId = 10, Quantity = 2 };

        _productRepository.Setup(x => x.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(new Product { Id = 10 });
        _itemRepository.Setup(x => x.GetItemAsync(10, 1, It.IsAny<CancellationToken>())).ReturnsAsync(existingItem);
        _unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _sut.DeleteAsync(10, 1);

        // Assert
        _itemRepository.Verify(x => x.DeleteAsync(existingItem, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllByProductAsync_ShouldThrowNotFoundException_WhenProductDoesNotExist()
    {
        // Arrange
        _productRepository.Setup(x => x.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        // Act
        Func<Task> act = async () => await _sut.GetAllByProductAsync(10);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*10*");
        _itemRepository.Verify(x => x.GetAllByProductAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenProductDoesNotExist()
    {
        // Arrange
        _productRepository.Setup(x => x.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        // Act
        Func<Task> act = async () => await _sut.GetByIdAsync(10, 1);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*10*");
        _itemRepository.Verify(x => x.GetItemAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenItemDoesNotExist()
    {
        // Arrange
        _productRepository.Setup(x => x.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(new Product { Id = 10 });
        _itemRepository.Setup(x => x.GetItemAsync(10, 1, It.IsAny<CancellationToken>())).ReturnsAsync((Item?)null);

        // Act
        Func<Task> act = async () => await _sut.GetByIdAsync(10, 1);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*1*");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowNotFoundException_WhenProductDoesNotExist()
    {
        // Arrange
        _productRepository.Setup(x => x.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        // Act
        Func<Task> act = async () => await _sut.CreateAsync(10, new CreateItemRequest { Quantity = 1 });

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*10*");
        _itemRepository.Verify(x => x.AddAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowNotFoundException_WhenProductDoesNotExist()
    {
        // Arrange
        _productRepository.Setup(x => x.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        // Act
        Func<Task> act = async () => await _sut.UpdateAsync(10, 1, new UpdateItemRequest { Quantity = 1 });

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*10*");
        _itemRepository.Verify(x => x.UpdateAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowNotFoundException_WhenProductDoesNotExist()
    {
        // Arrange
        _productRepository.Setup(x => x.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        // Act
        Func<Task> act = async () => await _sut.DeleteAsync(10, 1);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*10*");
        _itemRepository.Verify(x => x.DeleteAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

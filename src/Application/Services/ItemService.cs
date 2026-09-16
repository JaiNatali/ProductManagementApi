using Application.DTOs.Item;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;

namespace Application.Services;

public sealed class ItemService : IItemService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ItemService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ItemSummaryResponse>> GetAllByProductAsync(int productId, CancellationToken cancellationToken = default)
    {
        var productExists = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken);
        if (productExists is null)
        {
            throw new KeyNotFoundException($"Product with id {productId} was not found.");
        }

        var items = await _unitOfWork.Items.GetAllByProductAsync(productId, cancellationToken);
        return _mapper.Map<IReadOnlyList<ItemSummaryResponse>>(items);
    }

    public async Task<ItemResponse> GetByIdAsync(int productId, int itemId, CancellationToken cancellationToken = default)
    {
        var productExists = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken);
        if (productExists is null)
        {
            throw new KeyNotFoundException($"Product with id {productId} was not found.");
        }

        var item = await _unitOfWork.Items.GetItemAsync(productId, itemId, cancellationToken);
        if (item is null)
        {
            throw new KeyNotFoundException($"Item with id {itemId} was not found for product {productId}.");
        }

        return _mapper.Map<ItemResponse>(item);
    }

    public async Task<ItemResponse> CreateAsync(int productId, CreateItemRequest request, CancellationToken cancellationToken = default)
    {
        var productExists = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken);
        if (productExists is null)
        {
            throw new KeyNotFoundException($"Product with id {productId} was not found.");
        }

        var item = _mapper.Map<Item>(request);
        item.ProductId = productId;

        await _unitOfWork.Items.AddAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ItemResponse>(item);
    }

    public async Task<ItemResponse> UpdateAsync(int productId, int itemId, UpdateItemRequest request, CancellationToken cancellationToken = default)
    {
        var productExists = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken);
        if (productExists is null)
        {
            throw new KeyNotFoundException($"Product with id {productId} was not found.");
        }

        var existingItem = await _unitOfWork.Items.GetItemAsync(productId, itemId, cancellationToken);
        if (existingItem is null)
        {
            throw new KeyNotFoundException($"Item with id {itemId} was not found for product {productId}.");
        }

        existingItem.Quantity = request.Quantity;

        await _unitOfWork.Items.UpdateAsync(existingItem, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ItemResponse>(existingItem);
    }

    public async Task DeleteAsync(int productId, int itemId, CancellationToken cancellationToken = default)
    {
        var productExists = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken);
        if (productExists is null)
        {
            throw new KeyNotFoundException($"Product with id {productId} was not found.");
        }

        var existingItem = await _unitOfWork.Items.GetItemAsync(productId, itemId, cancellationToken);
        if (existingItem is null)
        {
            throw new KeyNotFoundException($"Item with id {itemId} was not found for product {productId}.");
        }

        await _unitOfWork.Items.DeleteAsync(existingItem, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

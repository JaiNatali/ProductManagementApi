using Application.DTOs.Item;

namespace Application.Interfaces;

public interface IItemService
{
    Task<IReadOnlyList<ItemSummaryResponse>> GetAllByProductAsync(int productId, CancellationToken cancellationToken = default);
    Task<ItemResponse> GetByIdAsync(int productId, int itemId, CancellationToken cancellationToken = default);
    Task<ItemResponse> CreateAsync(int productId, CreateItemRequest request, CancellationToken cancellationToken = default);
    Task<ItemResponse> UpdateAsync(int productId, int itemId, UpdateItemRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int productId, int itemId, CancellationToken cancellationToken = default);
}

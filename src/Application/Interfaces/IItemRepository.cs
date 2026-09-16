using Domain.Entities;

namespace Application.Interfaces;

public interface IItemRepository : IRepository<Item>
{
    Task<IReadOnlyList<Item>> GetAllByProductAsync(int productId, CancellationToken cancellationToken = default);
    Task<Item?> GetItemAsync(int productId, int itemId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int productId, int itemId, CancellationToken cancellationToken = default);
}

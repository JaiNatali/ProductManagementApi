using Domain.Entities;

namespace Application.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetProductWithItemsAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetAllProductsWithItemsAsync(CancellationToken cancellationToken = default);
    Task<bool> ProductExistsAsync(string productName, CancellationToken cancellationToken = default);
}

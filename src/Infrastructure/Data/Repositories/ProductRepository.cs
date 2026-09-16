using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

public sealed class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<Product?> GetProductWithItemsAsync(int id, CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking()
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Product>> GetAllProductsWithItemsAsync(CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking()
            .Include(p => p.Items)
            .ToListAsync(cancellationToken);

    public async Task<bool> ProductExistsAsync(string productName, CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking()
            .AnyAsync(p => p.ProductName == productName, cancellationToken);
}

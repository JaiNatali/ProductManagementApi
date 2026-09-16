using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

public sealed class ItemRepository : Repository<Item>, IItemRepository
{
    public ItemRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<Item>> GetAllByProductAsync(int productId, CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking()
            .Where(i => i.ProductId == productId)
            .ToListAsync(cancellationToken);

    public async Task<Item?> GetItemAsync(int productId, int itemId, CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(i => i.ProductId == productId && i.Id == itemId, cancellationToken);

    public async Task<bool> ExistsAsync(int productId, int itemId, CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking()
            .AnyAsync(i => i.ProductId == productId && i.Id == itemId, cancellationToken);
}

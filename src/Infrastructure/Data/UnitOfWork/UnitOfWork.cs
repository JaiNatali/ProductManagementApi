using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data.Repositories;

namespace Infrastructure.Data.UnitOfWork;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Products = new ProductRepository(_context);
        Items = new ItemRepository(_context);
        Users = new Repository<User>(_context);
        RefreshTokens = new Repository<RefreshToken>(_context);
    }

    public IProductRepository Products { get; }
    public IItemRepository Items { get; }
    public IRepository<User> Users { get; }
    public IRepository<RefreshToken> RefreshTokens { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);

    public void Dispose()
    {
        _context.Dispose();
    }
}

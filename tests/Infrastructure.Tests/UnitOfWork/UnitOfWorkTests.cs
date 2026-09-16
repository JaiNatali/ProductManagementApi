using Application.Interfaces;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Data.UnitOfWork;
using Infrastructure.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.UnitOfWork;

public class UnitOfWorkTests
{
    [Fact]
    public async Task SaveChangesAsync_ShouldPersistChanges()
    {
        await using var context = TestDbContextFactory.Create();
        using var sut = new Infrastructure.Data.UnitOfWork.UnitOfWork(context);

        await sut.Products.AddAsync(new Product { ProductName = "Tablet", CreatedBy = "test", CreatedOn = DateTime.UtcNow });
        await sut.SaveChangesAsync();

        var saved = await context.Products.FirstOrDefaultAsync(p => p.ProductName == "Tablet");
        saved.Should().NotBeNull();
    }

    [Fact]
    public void RepositoryResolution_ShouldReturnCorrectRepositories()
    {
        using var context = TestDbContextFactory.Create();
        using var sut = new Infrastructure.Data.UnitOfWork.UnitOfWork(context);

        sut.Products.Should().NotBeNull();
        sut.Items.Should().NotBeNull();
        sut.Users.Should().NotBeNull();
        sut.RefreshTokens.Should().NotBeNull();
    }
}

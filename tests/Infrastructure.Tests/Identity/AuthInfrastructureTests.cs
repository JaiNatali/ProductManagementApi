using Application.Interfaces;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Identity;

public class AuthInfrastructureTests
{
    [Fact]
    public async Task ApplicationDbContext_ShouldStoreRefreshTokensAndUsers()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);
        var user = new User { Id = 1, Username = "admin", Email = "admin@example.com", PasswordHash = "hash", Role = "Admin" };
        var token = new RefreshToken { Id = 10, UserId = 1, Token = "token", ExpiresOn = DateTime.UtcNow.AddDays(1), CreatedOn = DateTime.UtcNow, IsRevoked = false, User = user };

        // Act
        context.Users.Add(user);
        context.RefreshTokens.Add(token);
        await context.SaveChangesAsync();

        // Assert
        var savedUser = await context.Users.FindAsync(1);
        var savedToken = await context.RefreshTokens.FindAsync(10);
        savedUser.Should().NotBeNull();
        savedToken.Should().NotBeNull();
        savedToken!.Token.Should().Be("token");
    }
}

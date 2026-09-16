using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Tests.Identity;

public class PasswordHasherTests
{
    [Fact]
    public void HashPassword_ShouldGenerateDifferentHashForSamePassword()
    {
        var hasher = new PasswordHasher<User>();
        var user = new User { Id = 1, Username = "admin" };

        var hash1 = hasher.HashPassword(user, "Password123!");
        var hash2 = hasher.HashPassword(user, "Password123!");

        hash1.Should().NotBeNullOrWhiteSpace();
        hash2.Should().NotBeNullOrWhiteSpace();
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void VerifyHashedPassword_ShouldReturnSuccess_ForValidPassword()
    {
        var hasher = new PasswordHasher<User>();
        var user = new User { Id = 1, Username = "admin" };
        var hash = hasher.HashPassword(user, "Password123!");

        var result = hasher.VerifyHashedPassword(user, hash, "Password123!");

        result.Should().Be(PasswordVerificationResult.Success);
    }

    [Fact]
    public void VerifyHashedPassword_ShouldReturnFailed_ForInvalidPassword()
    {
        var hasher = new PasswordHasher<User>();
        var user = new User { Id = 1, Username = "admin" };
        var hash = hasher.HashPassword(user, "Password123!");

        var result = hasher.VerifyHashedPassword(user, hash, "WrongPassword!");

        result.Should().Be(PasswordVerificationResult.Failed);
    }
}

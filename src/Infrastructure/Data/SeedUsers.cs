using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Data;

public static class SeedUsers
{
    public static async Task SeedAsync(ApplicationDbContext context, IConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var passwordHasher = new PasswordHasher<User>();

        var adminUser = await context.Users.SingleOrDefaultAsync(u => u.Email == "admin@yopmail.com", cancellationToken);
        if (adminUser is null)
        {
            var adminPassword = GetRequiredPassword(configuration, "SeedUsers:AdminPassword");
            adminUser = new User
            {
                Username = "admin",
                Email = "admin@yopmail.com",
                Role = "Admin",
                CreatedOn = DateTime.UtcNow,
                IsActive = true
            };
            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, adminPassword);
            await context.Users.AddAsync(adminUser, cancellationToken);
        }

        adminUser.Username = "admin";
        adminUser.Role = "Admin";
        adminUser.IsActive = true;
        if (adminUser.CreatedOn == default)
        {
            adminUser.CreatedOn = DateTime.UtcNow;
        }

        var normalUser = await context.Users.SingleOrDefaultAsync(u => u.Email == "user@yopmail.com", cancellationToken);
        if (normalUser is null)
        {
            var userPassword = GetRequiredPassword(configuration, "SeedUsers:UserPassword");
            normalUser = new User
            {
                Username = "user",
                Email = "user@yopmail.com",
                Role = "User",
                CreatedOn = DateTime.UtcNow,
                IsActive = true
            };
            normalUser.PasswordHash = passwordHasher.HashPassword(normalUser, userPassword);
            await context.Users.AddAsync(normalUser, cancellationToken);
        }

        normalUser.Username = "user";
        normalUser.Role = "User";
        normalUser.IsActive = true;
        if (normalUser.CreatedOn == default)
        {
            normalUser.CreatedOn = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static string GetRequiredPassword(IConfiguration configuration, string key)
    {
        var password = configuration[key];
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException($"Required seed user password configuration is missing: {key}.");
        }

        return password;
    }
}

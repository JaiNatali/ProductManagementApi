using Application.DTOs.Auth;
using Application.Interfaces;
using Application.Options;
using Application.Services;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Application.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IJwtTokenService> _jwtTokenService = new();
    private readonly Mock<IRepository<User>> _userRepository = new();
    private readonly Mock<IRepository<RefreshToken>> _refreshTokenRepository = new();
    private readonly Mock<ILogger<AuthService>> _logger = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _unitOfWork.SetupGet(x => x.Users).Returns(_userRepository.Object);
        _unitOfWork.SetupGet(x => x.RefreshTokens).Returns(_refreshTokenRepository.Object);

        var jwtOptions = Microsoft.Extensions.Options.Options.Create(new JwtOptions { AccessTokenExpirationMinutes = 15, RefreshTokenExpirationDays = 7 });
        _sut = new AuthService(_unitOfWork.Object, _jwtTokenService.Object, jwtOptions, _logger.Object);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnTokens_WhenCredentialsAreValid()
    {
        // Arrange
        var user = new User { Id = 1, Username = "admin", Email = "admin@example.com", PasswordHash = new PasswordHasher<User>().HashPassword(null!, "Password123!"), Role = "Admin", IsActive = true };
        var request = new LoginRequest { Email = user.Email, Password = "Password123!" };

        _userRepository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _jwtTokenService.Setup(x => x.GenerateAccessToken(user.Id, user.Username, user.Email, user.Role)).Returns("access-token");
        _jwtTokenService.Setup(x => x.GenerateRefreshToken()).Returns("refresh-token");
        _unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.LoginAsync(request);

        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        result.ExpiresIn.Should().Be(900);
        _refreshTokenRepository.Verify(x => x.AddAsync(It.Is<RefreshToken>(rt => rt.UserId == user.Id && rt.Token == "refresh-token"), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorizedAccessException_WhenPasswordIsInvalid()
    {
        var user = new User { Id = 1, Username = "admin", Email = "admin@example.com", PasswordHash = new PasswordHasher<User>().HashPassword(null!, "Password123!"), Role = "Admin", IsActive = true };
        var request = new LoginRequest { Email = user.Email, Password = "WrongPassword!" };

        _userRepository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        Func<Task> act = async () => await _sut.LoginAsync(request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("*Invalid email or password*");
        _refreshTokenRepository.Verify(x => x.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorizedAccessException_WhenUserDoesNotExist()
    {
        var request = new LoginRequest { Email = "missing@example.com", Password = "Password123!" };
        _userRepository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        Func<Task> act = async () => await _sut.LoginAsync(request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("*Invalid email or password*");
        _refreshTokenRepository.Verify(x => x.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldRotateRefreshToken_WhenTokenIsValid()
    {
        var user = new User { Id = 1, Username = "admin", Email = "admin@example.com", Role = "Admin", IsActive = true };
        var existingToken = new RefreshToken { Id = 11, UserId = user.Id, Token = "old-token", ExpiresOn = DateTime.UtcNow.AddDays(1), IsRevoked = false };

        _refreshTokenRepository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingToken);
        _userRepository.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _jwtTokenService.Setup(x => x.GenerateRefreshToken()).Returns("new-token");
        _jwtTokenService.Setup(x => x.GenerateAccessToken(user.Id, user.Username, user.Email, user.Role)).Returns("new-access-token");
        _unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.RefreshTokenAsync(new RefreshTokenRequest { RefreshToken = "old-token" });

        result.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().Be("new-token");
        existingToken.IsRevoked.Should().BeTrue();
        _refreshTokenRepository.Verify(x => x.UpdateAsync(existingToken, It.IsAny<CancellationToken>()), Times.Once);
        _refreshTokenRepository.Verify(x => x.AddAsync(It.Is<RefreshToken>(rt => rt.Token == "new-token" && rt.UserId == user.Id), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldThrowUnauthorizedAccessException_WhenTokenIsExpired()
    {
        var existingToken = new RefreshToken { Token = "old-token", ExpiresOn = DateTime.UtcNow.AddMinutes(-1), IsRevoked = false };
        _refreshTokenRepository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingToken);

        Func<Task> act = async () => await _sut.RefreshTokenAsync(new RefreshTokenRequest { RefreshToken = "old-token" });

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("*invalid or expired*");
        _refreshTokenRepository.Verify(x => x.UpdateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldThrowUnauthorizedAccessException_WhenTokenIsRevoked()
    {
        var existingToken = new RefreshToken { Token = "old-token", ExpiresOn = DateTime.UtcNow.AddDays(1), IsRevoked = true };
        _refreshTokenRepository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingToken);

        Func<Task> act = async () => await _sut.RefreshTokenAsync(new RefreshTokenRequest { RefreshToken = "old-token" });

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("*invalid or expired*");
        _refreshTokenRepository.Verify(x => x.UpdateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LogoutAsync_ShouldRevokeRefreshToken_WhenTokenExists()
    {
        var existingToken = new RefreshToken { Id = 11, UserId = 1, Token = "token", IsRevoked = false };
        _refreshTokenRepository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingToken);
        _unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await _sut.LogoutAsync("token");

        existingToken.IsRevoked.Should().BeTrue();
        _refreshTokenRepository.Verify(x => x.UpdateAsync(existingToken, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

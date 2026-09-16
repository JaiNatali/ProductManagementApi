using Application.Options;
using Application.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;

namespace Infrastructure.Tests.Identity;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _sut;

    public JwtTokenServiceTests()
    {
        var options = Options.Create(new JwtOptions
        {
            SecretKey = "SuperSecretKey1234567890SuperSecretKey1234567890",
            Issuer = "test-issuer",
            Audience = "test-audience",
            AccessTokenExpirationMinutes = 60,
            RefreshTokenExpirationDays = 7
        });

        _sut = new JwtTokenService(options);
    }

    [Fact]
    public void GenerateAccessToken_ShouldReturnValidJwt()
    {
        var token = _sut.GenerateAccessToken(1, "admin", "admin@example.com", "Admin");

        token.Should().NotBeNullOrWhiteSpace();
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Issuer.Should().Be("test-issuer");
        jwt.Audiences.Should().Contain("test-audience");
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti);
        jwt.Claims.Should().Contain(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier && c.Value == "1");
        jwt.Claims.Should().Contain(c => c.Type == System.Security.Claims.ClaimTypes.Email && c.Value == "admin@example.com");
        jwt.Claims.Should().Contain(c => c.Type == System.Security.Claims.ClaimTypes.Role && c.Value == "Admin");
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnBase64StringOfLengthGreaterThanZero()
    {
        var token = _sut.GenerateRefreshToken();

        token.Should().NotBeNullOrWhiteSpace();
        Convert.FromBase64String(token).Should().NotBeEmpty();
    }
}

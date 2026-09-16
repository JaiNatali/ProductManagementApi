namespace Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(int userId, string username, string email, string role);
    string GenerateRefreshToken();
}

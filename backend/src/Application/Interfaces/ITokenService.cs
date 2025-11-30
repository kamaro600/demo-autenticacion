using Domain.Entities;

namespace Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    Task<RefreshToken> CreateRefreshTokenAsync(string userId);
    Task<bool> ValidateRefreshTokenAsync(string token);
    Task<User?> GetUserFromRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token, string? replacedByToken = null);
    Task RevokeAllUserRefreshTokensAsync(string userId);
}
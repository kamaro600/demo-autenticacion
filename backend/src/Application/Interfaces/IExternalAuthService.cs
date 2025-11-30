using Application.DTOs.Auth;

namespace Application.Interfaces;

public interface IExternalAuthService
{
    Task<AuthResponse> GoogleLoginAsync(string accessToken);
    Task<AuthResponse> GitHubLoginAsync(string accessToken);
    Task<AuthResponse> DiscordLoginAsync(string accessToken);
    Task<ExternalLoginInfo?> GetGoogleUserInfoAsync(string accessToken);
    Task<ExternalLoginInfo?> GetGitHubUserInfoAsync(string accessToken);
    Task<ExternalLoginInfo?> GetDiscordUserInfoAsync(string accessToken);
    Task<string?> ExchangeGitHubCodeAsync(string code);
    Task<string?> ExchangeDiscordCodeAsync(string code);
}
using System.Text.Json;
using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class ExternalAuthService : IExternalAuthService
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;
    private readonly IMfaService _mfaService;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalAuthService> _logger;
    private readonly IConfiguration _configuration;

    public ExternalAuthService(
        IUserService userService,
        ITokenService tokenService,
        IMfaService mfaService,
        HttpClient httpClient,
        ILogger<ExternalAuthService> logger,
        IConfiguration configuration)
    {
        _userService = userService;
        _tokenService = tokenService;
        _mfaService = mfaService;
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<AuthResponse> GoogleLoginAsync(string accessToken)
    {
        var userInfo = await GetGoogleUserInfoAsync(accessToken);
        if (userInfo == null)
        {
            return new AuthResponse(false, Message: "Falla al obtener informacion de usuario desde Google");
        }

        return await ProcessExternalLoginAsync(userInfo);
    }

    public async Task<AuthResponse> GitHubLoginAsync(string accessToken)
    {
        var userInfo = await GetGitHubUserInfoAsync(accessToken);
        if (userInfo == null)
        {
            return new AuthResponse(false, Message: "Falla al obtener informacion de usuario desde GITHUB");
        }

        return await ProcessExternalLoginAsync(userInfo);
    }

    public async Task<string?> ExchangeGitHubCodeAsync(string code)
    {
        try
        {
            var clientId = _configuration["OAuth:GitHub:ClientId"];
            var clientSecret = _configuration["OAuth:GitHub:ClientSecret"];

            var requestData = new
            {
                client_id = clientId,
                client_secret = clientSecret,
                code = code
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/oauth/access_token");
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(
                JsonSerializer.Serialize(requestData),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Intercambio de token GitHub fallo: {responseContent}");
                return null;
            }

            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            var accessToken = tokenResponse.GetProperty("access_token").GetString();

            return accessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en el Intercambio de token GitHub");
            return null;
        }
    }

    public async Task<AuthResponse> DiscordLoginAsync(string accessToken)
    {
        var userInfo = await GetDiscordUserInfoAsync(accessToken);
        if (userInfo == null)
        {
            return new AuthResponse(false, Message: "Falla al obtener informacion de usuario desde Discord");
        }

        return await ProcessExternalLoginAsync(userInfo);
    }

    public async Task<string?> ExchangeDiscordCodeAsync(string code)
    {
        try
        {
            var clientId = _configuration["OAuth:Discord:ClientId"];
            var clientSecret = _configuration["OAuth:Discord:ClientSecret"];
            var redirectUri = _configuration["OAuth:Discord:RedirectUri"];

            var requestData = new Dictionary<string, string>
            {
                ["client_id"] = clientId!,
                ["client_secret"] = clientSecret!,
                ["code"] = code,
                ["redirect_uri"] = redirectUri!,
                ["grant_type"] = "authorization_code"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://discord.com/api/oauth2/token");
            request.Content = new FormUrlEncodedContent(requestData);

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Error en el Intercambio de token Discord: {responseContent}");
                return null;
            }

            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            var accessToken = tokenResponse.GetProperty("access_token").GetString();

            return accessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en el Intercambio de token Discord");
            return null;
        }
    }

    public async Task<ExternalLoginInfo?> GetGoogleUserInfoAsync(string accessToken)
    {
        try
        {
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(accessToken) as System.IdentityModel.Tokens.Jwt.JwtSecurityToken;
            
            if (jsonToken == null)
            {
                _logger.LogError("Token JWT de Google invalido ");
                return null;
            }

            var userId = jsonToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var email = jsonToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var name = jsonToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name))
            {
                _logger.LogError("Faltan claims requeridos del Google JWT token");
                return null;
            }
            
            return new ExternalLoginInfo(
                "Google",
                userId,
                email,
                name
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener informacion de Google");
            return null;
        }
    }

    public async Task<ExternalLoginInfo?> GetGitHubUserInfoAsync(string accessToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new("Bearer", accessToken);
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Demo-Auth-App");

            var response = await _httpClient.GetStringAsync("https://api.github.com/user");
            var userInfo = JsonSerializer.Deserialize<JsonElement>(response);
            
            // Get email from separate endpoint if not public
            var email = userInfo.TryGetProperty("email", out var emailProp) && !emailProp.ValueKind.Equals(JsonValueKind.Null)
                ? emailProp.GetString()
                : await GetGitHubEmailAsync(accessToken);

            return new ExternalLoginInfo(
                "GitHub",
                userInfo.GetProperty("id").GetInt32().ToString(),
                email!,
                userInfo.GetProperty("name").GetString() ?? userInfo.GetProperty("login").GetString()!
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener informacion de usuario Github");
            return null;
        }
    }

    public async Task<ExternalLoginInfo?> GetDiscordUserInfoAsync(string accessToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new("Bearer", accessToken);

            var response = await _httpClient.GetStringAsync("https://discord.com/api/users/@me");
            var userInfo = JsonSerializer.Deserialize<JsonElement>(response);
            
            var userId = userInfo.GetProperty("id").GetString();
            var username = userInfo.GetProperty("username").GetString();
            var discriminator = userInfo.TryGetProperty("discriminator", out var discProp) ? discProp.GetString() : null;
            var email = userInfo.TryGetProperty("email", out var emailProp) && !emailProp.ValueKind.Equals(JsonValueKind.Null)
                ? emailProp.GetString()
                : null;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(username))
            {
                _logger.LogError("Faltan datos requeridos de Discord");
                return null;
            }

            var userEmail = email ?? $"{username}@discord.local";
            var displayName = discriminator != null && discriminator != "0" 
                ? $"{username}#{discriminator}" 
                : username;

            return new ExternalLoginInfo(
                "Discord",
                userId,
                userEmail,
                displayName
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener informacion de usuario Discord");
            return null;
        }
    }

    private async Task<string?> GetGitHubEmailAsync(string accessToken)
    {
        try
        {
            var response = await _httpClient.GetStringAsync("https://api.github.com/user/emails");
            var emails = JsonSerializer.Deserialize<JsonElement[]>(response);
            
            var primaryEmail = emails.FirstOrDefault(e => e.GetProperty("primary").GetBoolean());
            return primaryEmail.GetProperty("email").GetString();
        }
        catch
        {
            return null;
        }
    }

    private async Task<AuthResponse> ProcessExternalLoginAsync(ExternalLoginInfo loginInfo)
    {
        try
        {

            var existingLogin = await _userService.GetExternalLoginAsync(loginInfo.Provider, loginInfo.ProviderUserId);
            User user;

            if (existingLogin != null)
            {
                user = await _userService.GetByIdAsync(existingLogin.UserId) ?? throw new InvalidOperationException("User not found");
            }
            else
            {

                user = await _userService.GetByEmailAsync(loginInfo.Email);
                
                if (user == null)
                {
                    // Create new user
                    var nameParts = loginInfo.Name.Split(' ', 2);
                    user = new User
                    {
                        FirstName = nameParts.Length > 0 ? nameParts[0] : loginInfo.Name,
                        LastName = nameParts.Length > 1 ? nameParts[1] : "",
                        Email = loginInfo.Email,
                        UserName = loginInfo.Email,
                        EmailConfirmed = true
                    };

                    user = await _userService.CreateAsync(user, Guid.NewGuid().ToString());
                }


                var externalLogin = new ExternalLogin
                {
                    UserId = user.Id,
                    Provider = loginInfo.Provider,
                    ProviderUserId = loginInfo.ProviderUserId,
                    Email = loginInfo.Email,
                    Name = loginInfo.Name
                };

                await _userService.AddExternalLoginAsync(user, externalLogin);
            }


            var mfaStatus = await _mfaService.GetMfaStatusAsync(user.Id);
            

            if (mfaStatus.IsEnabled)
            {
                var userDto = new UserDto(
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email!,
                    mfaStatus.IsEnabled
                );

                return new AuthResponse(
                    false,
                    null,
                    null,
                    null,
                    userDto,
                    "Codigo MFA requerido",
                    true
                );
            }
            
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = await _tokenService.CreateRefreshTokenAsync(user.Id);

            var userDtoSuccess = new UserDto(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email!,
                mfaStatus.IsEnabled
            );

            return new AuthResponse(
                true,
                accessToken,
                refreshToken.Token,
                DateTime.UtcNow.AddMinutes(15),
                userDtoSuccess,
                "Logueo externo exitoso"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante logueo externo");
            return new AuthResponse(false, Message: "Error durante logueo externo");
        }
    }
}
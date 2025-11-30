using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;
    private readonly IMfaService _mfaService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserService userService,
        ITokenService tokenService,
        IMfaService mfaService,
        ILogger<AuthService> logger)
    {
        _userService = userService;
        _tokenService = tokenService;
        _mfaService = mfaService;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            if (request.Password != request.ConfirmPassword)
            {
                return new AuthResponse(false, Message: "Password invalido.");
            }

            if (await _userService.ExistsAsync(request.Email))
            {
                return new AuthResponse(false, Message: "Usuario con el email ya existe");
            }

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                UserName = request.Email,
                EmailConfirmed = true
            };

            var createdUser = await _userService.CreateAsync(user, request.Password);
            if (createdUser == null)
            {
                return new AuthResponse(false, Message: "Fallo al crear usuario");
            }

            var accessToken = _tokenService.GenerateAccessToken(createdUser);
            var refreshToken = await _tokenService.CreateRefreshTokenAsync(createdUser.Id);

            var userDto = new UserDto(
                createdUser.Id,
                createdUser.FirstName,
                createdUser.LastName,
                createdUser.Email!,
                false
            );

            return new AuthResponse(
                true,
                accessToken,
                refreshToken.Token,
                DateTime.UtcNow.AddMinutes(15),
                userDto,
                "Usuario registrado"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el registro");
            return new AuthResponse(false, Message: "Error durante el registro");
        }
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _userService.GetByEmailAsync(request.Email);
            if (user == null || !await _userService.ValidatePasswordAsync(user, request.Password))
            {
                return new AuthResponse(false, Message: "Email o password invalido");
            }

            // Check if MFA is enabled
            var mfaStatus = await _mfaService.GetMfaStatusAsync(user.Id);
            if (mfaStatus.IsEnabled)
            {
                if (string.IsNullOrEmpty(request.MfaCode))
                {
                    return new AuthResponse(false, RequiresMfa: true, Message: "Codigo MFA requerido");
                }

                var mfaVerification = await _mfaService.VerifyMfaAsync(new(user.Id, request.MfaCode));
                if (!mfaVerification.Success)
                {
                    return new AuthResponse(false, Message: "Codigo MFA invalido");
                }
            }

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = await _tokenService.CreateRefreshTokenAsync(user.Id);

            var userDto = new UserDto(
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
                userDto,
                "Inicio de sesion exitoso"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante login");
            return new AuthResponse(false, Message: "Error durante login");
        }
    }

    public async Task<AuthResponse> VerifyMfaAndLoginAsync(MfaLoginRequest request)
    {
        try
        {
            var user = await _userService.GetByIdAsync(request.UserId);
            if (user == null)
            {
                return new AuthResponse(false, Message: "Usuario no encontrado");
            }

            var mfaVerification = await _mfaService.VerifyMfaAsync(new(user.Id, request.MfaCode));
            if (!mfaVerification.Success)
            {
                return new AuthResponse(false, Message: "Codigo MFA invalido");
            }

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = await _tokenService.CreateRefreshTokenAsync(user.Id);

            var mfaStatus = await _mfaService.GetMfaStatusAsync(user.Id);
            var userDto = new UserDto(
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
                userDto,
                "Login successful"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante validacion de MFA  y login");
            return new AuthResponse(false, Message: "Error durante validacion de MFA  y login");
        }
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        try
        {
            if (!await _tokenService.ValidateRefreshTokenAsync(request.RefreshToken))
            {
                return new AuthResponse(false, Message: "Refesh token invalido");
            }

            var user = await _tokenService.GetUserFromRefreshTokenAsync(request.RefreshToken);
            if (user == null)
            {
                return new AuthResponse(false, Message: "Usuario no encontrado");
            }

            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = await _tokenService.CreateRefreshTokenAsync(user.Id);

            // Revoke old refresh token
            await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken, newRefreshToken.Token);

            var mfaStatus = await _mfaService.GetMfaStatusAsync(user.Id);
            var userDto = new UserDto(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email!,
                mfaStatus.IsEnabled
            );

            return new AuthResponse(
                true,
                newAccessToken,
                newRefreshToken.Token,
                DateTime.UtcNow.AddMinutes(15),
                userDto,
                "Token refrescado correctamente"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el refresco de token");
            return new AuthResponse(false, Message: "Error durante el refresco de token");
        }
    }

    public async Task<bool> LogoutAsync(string userId, string? refreshToken = null)
    {
        try
        {
            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _tokenService.RevokeRefreshTokenAsync(refreshToken);
            }
            else
            {
                await _tokenService.RevokeAllUserRefreshTokensAsync(userId);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el logout");
            return false;
        }
    }

    public async Task<bool> RevokeAllTokensAsync(string userId)
    {
        try
        {
            await _tokenService.RevokeAllUserRefreshTokensAsync(userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al revocar los tokens");
            return false;
        }
    }
}
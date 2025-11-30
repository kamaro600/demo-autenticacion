using Application.DTOs.Auth;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IExternalAuthService _externalAuthService;

    public AuthController(IAuthService authService, IExternalAuthService externalAuthService)
    {
        _authService = authService;
        _externalAuthService = externalAuthService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        
  
        if (result.RequiresMfa)
        {
            return Ok(result);
        }
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest? request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("Usuario invalido");
        }

        var success = await _authService.LogoutAsync(userId, request?.RefreshToken);
        
        if (success)
        {
            return Ok(new { message = "Cierre de sesion exitoso" });
        }
        
        return BadRequest(new { message = "Cierre de sesion fallo" });
    }

    [HttpPost("external/google")]
    public async Task<IActionResult> GoogleLogin([FromBody] ExternalLoginRequest request)
    {
        var result = await _externalAuthService.GoogleLoginAsync(request.AccessToken);
        
        if (result.RequiresMfa)
        {
            return Ok(result);
        }
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    [HttpPost("external/github")]
    public async Task<IActionResult> GitHubLogin([FromBody] ExternalLoginRequest request)
    {
        string accessToken = request.AccessToken;
        
        if (request.AccessToken.Length < 40 && !request.AccessToken.StartsWith("gho_"))
        {
            var exchangedToken = await _externalAuthService.ExchangeGitHubCodeAsync(request.AccessToken);
            if (exchangedToken == null)
            {
                return BadRequest(new { message = "Fallo al intercambiar el codigo github por el acces token" });
            }
            accessToken = exchangedToken;
        }
        
        var result = await _externalAuthService.GitHubLoginAsync(accessToken);
        
        if (result.RequiresMfa)
        {
            return Ok(result);
        }
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    [HttpPost("external/discord")]
    public async Task<IActionResult> DiscordLogin([FromBody] ExternalLoginRequest request)
    {
        string accessToken = request.AccessToken;
        
        if (request.AccessToken.Length < 100)
        {
            var exchangedToken = await _externalAuthService.ExchangeDiscordCodeAsync(request.AccessToken);
            if (exchangedToken == null)
            {
                return BadRequest(new { message = "Fallo al intercambiar el codigo github por el acces token" });
            }
            accessToken = exchangedToken;
        }
        
        var result = await _externalAuthService.DiscordLoginAsync(accessToken);
        
        if (result.RequiresMfa)
        {
            return Ok(result);
        }
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    [HttpPost("mfa/verify-login")]
    public async Task<IActionResult> VerifyMfaLogin([FromBody] MfaLoginRequest request)
    {
        var result = await _authService.VerifyMfaAndLoginAsync(request);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    [HttpPost("revoke-all-tokens")]
    [Authorize]
    public async Task<IActionResult> RevokeAllTokens()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("Usuario invalido");
        }

        var success = await _authService.RevokeAllTokensAsync(userId);
        
        if (success)
        {
            return Ok(new { message = "Todos los tokens fueron removidos" });
        }
        
        return BadRequest(new { message = "Fallo al remover tokens" });
    }
}
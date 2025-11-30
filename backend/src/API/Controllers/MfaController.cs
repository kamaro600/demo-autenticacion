using Application.DTOs.Mfa;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MfaController : ControllerBase
{
    private readonly IMfaService _mfaService;

    public MfaController(IMfaService mfaService)
    {
        _mfaService = mfaService;
    }

    [HttpPost("setup")]
    public async Task<IActionResult> SetupMfa()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("Usuario invalido");
        }

        var result = await _mfaService.SetupMfaAsync(new MfaSetupRequest(userId));
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    [HttpPost("enable")]
    public async Task<IActionResult> EnableMfa([FromBody] MfaEnableRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("Usuario invalido");
        }

        var verifyRequest = new MfaVerifyRequest(userId, request.Code);
        var result = await _mfaService.EnableMfaAsync(verifyRequest);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    [HttpPost("verify")]
    public async Task<IActionResult> VerifyMfa([FromBody] MfaEnableRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("Usuario invalido");
        }

        var verifyRequest = new MfaVerifyRequest(userId, request.Code);
        var result = await _mfaService.VerifyMfaAsync(verifyRequest);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    [HttpDelete("disable")]
    public async Task<IActionResult> DisableMfa()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("Usuario invalido");
        }

        var success = await _mfaService.DisableMfaAsync(userId);
        
        if (success)
        {
            return Ok(new { message = "MFA desabilitado" });
        }
        
        return BadRequest(new { message = "Fallo al deshabiltiar MFA" });
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetMfaStatus()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("Usuario invalido");
        }

        var status = await _mfaService.GetMfaStatusAsync(userId);
        return Ok(status);
    }
}
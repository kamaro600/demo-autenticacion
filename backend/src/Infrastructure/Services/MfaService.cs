using System.Text;
using Application.DTOs.Mfa;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OtpNet;
using QRCoder;

namespace Infrastructure.Services;

public class MfaService : IMfaService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public MfaService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<MfaSetupResponse> SetupMfaAsync(MfaSetupRequest request)
    {
        try
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                return new MfaSetupResponse(false, Message: "Usuario no encontrado");
            }

            var existingMfa = await _context.UserMfas
                .FirstOrDefaultAsync(m => m.UserId == request.UserId);

            string secretKey;
            if (existingMfa != null && !existingMfa.IsEnabled)
            {
                secretKey = existingMfa.SecretKey;
            }
            else if (existingMfa != null && existingMfa.IsEnabled)
            {
                return new MfaSetupResponse(false, Message: "El MFA ya esta habilitado para el usuario");
            }
            else
            {
                secretKey = GenerateSecretKey();

                var userMfa = new UserMfa
                {
                    UserId = request.UserId,
                    SecretKey = secretKey,
                    IsEnabled = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.UserMfas.Add(userMfa);
                await _context.SaveChangesAsync();
            }

            var qrCodeUri = GenerateQrCodeUri(user.Email!, secretKey);
            var qrCodeBase64 = GenerateQrCodeBase64(qrCodeUri);

            return new MfaSetupResponse(
                true,
                qrCodeBase64,
                secretKey,
                "Configuracion MFA exitoso. Escanea el QR."
            );
        }
        catch (Exception ex)
        {
            return new MfaSetupResponse(false, Message: $"Error al configurar MFA: {ex.Message}");
        }
    }

    public async Task<MfaVerifyResponse> VerifyMfaAsync(MfaVerifyRequest request)
    {
        try
        {
            var userMfa = await _context.UserMfas
                .FirstOrDefaultAsync(m => m.UserId == request.UserId && m.IsEnabled);

            if (userMfa == null)
            {
                return new MfaVerifyResponse(false, "MFA no esta habilitado para el usuario");
            }

            var isValid = VerifyCode(userMfa.SecretKey, request.Code);
            return new MfaVerifyResponse(
                isValid,
                isValid ? "Verificacion MFA exitosa" : "Codigo MFA invalido"
            );
        }
        catch (Exception ex)
        {
            return new MfaVerifyResponse(false, $"Error  MFA: {ex.Message}");
        }
    }

    public async Task<MfaVerifyResponse> EnableMfaAsync(MfaVerifyRequest request)
    {
        try
        {
            var userMfa = await _context.UserMfas
                .FirstOrDefaultAsync(m => m.UserId == request.UserId);

            if (userMfa == null)
            {
                return new MfaVerifyResponse(false, "Configuracion MFA no encontrada");
            }

            if (userMfa.IsEnabled)
            {
                return new MfaVerifyResponse(false, "MFA ya esta habilitada");
            }

            var isValid = VerifyCode(userMfa.SecretKey, request.Code);
            if (!isValid)
            {
                return new MfaVerifyResponse(false, "Codigo MFA invalido");
            }

            userMfa.IsEnabled = true;
            userMfa.EnabledAt = DateTime.UtcNow;
            userMfa.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new MfaVerifyResponse(true, "MFA habilitado satisfactoriamente");
        }
        catch (Exception ex)
        {
            return new MfaVerifyResponse(false, $"Error MFA: {ex.Message}");
        }
    }

    public async Task<bool> DisableMfaAsync(string userId)
    {
        try
        {
            var userMfa = await _context.UserMfas
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (userMfa != null)
            {
                _context.UserMfas.Remove(userMfa);
                await _context.SaveChangesAsync();
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<MfaStatusResponse> GetMfaStatusAsync(string userId)
    {
        var userMfa = await _context.UserMfas
            .FirstOrDefaultAsync(m => m.UserId == userId);

        return new MfaStatusResponse(
            userMfa?.IsEnabled ?? false,
            userMfa?.EnabledAt
        );
    }

    public string GenerateSecretKey()
    {
        var key = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(key);
    }

    public string GenerateQrCodeUri(string email, string secretKey)
    {
        var appName = _configuration["App:Name"] ?? "Demo Auth App";
        return $"otpauth://totp/{Uri.EscapeDataString(appName)}:{Uri.EscapeDataString(email)}?secret={secretKey}&issuer={Uri.EscapeDataString(appName)}";
    }

    public bool VerifyCode(string secretKey, string code)
    {
        try
        {
            var secretKeyBytes = Base32Encoding.ToBytes(secretKey);
            var totp = new Totp(secretKeyBytes);
            
            return totp.VerifyTotp(code, out _, new VerificationWindow(1, 1));
        }
        catch
        {
            return false;
        }
    }

    private string GenerateQrCodeBase64(string qrCodeUri)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(qrCodeUri, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        
        var qrCodeBytes = qrCode.GetGraphic(20);
        return Convert.ToBase64String(qrCodeBytes);
    }
}
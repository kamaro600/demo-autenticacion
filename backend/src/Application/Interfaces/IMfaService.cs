using Application.DTOs.Mfa;

namespace Application.Interfaces;

public interface IMfaService
{
    Task<MfaSetupResponse> SetupMfaAsync(MfaSetupRequest request);
    Task<MfaVerifyResponse> VerifyMfaAsync(MfaVerifyRequest request);
    Task<MfaVerifyResponse> EnableMfaAsync(MfaVerifyRequest request);
    Task<bool> DisableMfaAsync(string userId);
    Task<MfaStatusResponse> GetMfaStatusAsync(string userId);
    string GenerateSecretKey();
    string GenerateQrCodeUri(string email, string secretKey);
    bool VerifyCode(string secretKey, string code);
}
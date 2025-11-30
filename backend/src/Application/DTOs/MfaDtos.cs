namespace Application.DTOs.Mfa;

public record MfaSetupRequest(
    string UserId
);

public record MfaSetupResponse(
    bool Success,
    string? QrCodeBase64 = null,
    string? ManualEntryKey = null,
    string? Message = null
);

public record MfaEnableRequest(
    string Code
);


public record MfaVerifyRequest(
    string UserId,
    string Code
);

public record MfaVerifyResponse(
    bool Success,
    string? Message = null
);

public record MfaStatusResponse(
    bool IsEnabled,
    DateTime? EnabledAt = null
);
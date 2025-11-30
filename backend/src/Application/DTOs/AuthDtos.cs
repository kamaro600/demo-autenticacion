namespace Application.DTOs.Auth;

public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string ConfirmPassword
);

public record LoginRequest(
    string Email,
    string Password,
    string? MfaCode = null
);

public record MfaLoginRequest(
    string UserId,
    string MfaCode
);

public record AuthResponse(
    bool Success,
    string? AccessToken = null,
    string? RefreshToken = null,
    DateTime? AccessTokenExpiry = null,
    UserDto? User = null,
    string? Message = null,
    bool RequiresMfa = false
);

public record UserDto(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    bool HasMfaEnabled
);

public record RefreshTokenRequest(
    string RefreshToken
);

public record ExternalLoginRequest(
    string Provider,
    string AccessToken
);

public record ExternalLoginInfo(
    string Provider,
    string ProviderUserId,
    string Email,
    string Name
);
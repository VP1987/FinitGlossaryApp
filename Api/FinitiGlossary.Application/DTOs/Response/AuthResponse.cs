namespace FinitiGlossary.Application.DTOs.Response;
public record AuthResponse(bool Success,
    string Token,
    string RefreshToken,
    string Message,
    UserFlags? Flags);

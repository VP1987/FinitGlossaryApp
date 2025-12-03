namespace FinitiGlossary.Application.DTOs.Response;

public record RefreshTokenResponse(bool Success, string Token, string RefreshToken, string Message);

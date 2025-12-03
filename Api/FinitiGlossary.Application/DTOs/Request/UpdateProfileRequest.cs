namespace FinitiGlossary.Application.DTOs.Request;

public record UpdateProfileRequest(int UserId,
string? NewUsername,
string? NewEmail,
string? NewPassword);

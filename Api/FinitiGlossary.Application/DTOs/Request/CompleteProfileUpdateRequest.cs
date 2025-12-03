namespace FinitiGlossary.Application.DTOs.Request;

public class CompleteProfileUpdateRequest
{
    public int UserId { get; set; }
    public string NewUsername { get; set; } = string.Empty;
    public string NewEmail { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

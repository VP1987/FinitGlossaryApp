namespace FinitiGlossary.Application.DTOs.Request
{
    public sealed record
        GetTermsAdminRequest(int? UserId, string? Role);

}

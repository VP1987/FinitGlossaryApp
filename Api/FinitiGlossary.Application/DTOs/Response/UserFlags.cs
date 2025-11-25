namespace FinitiGlossary.Application.DTOs.Response;

public record UserFlags(
bool MustChangePassword,
bool MustUpdateProfile
);

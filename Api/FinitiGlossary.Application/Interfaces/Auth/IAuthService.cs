using FinitiGlossary.Application.DTOs.Request;
using FinitiGlossary.Application.DTOs.Response;
using FinitiGlossary.Domain.Entities.Users;

namespace FinitiGlossary.Application.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<UnifiedResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken);
        Task<UnifiedResponse> ResetPasswordRequestAsync(string email);
        Task<UnifiedResponse> ResetPasswordConfirmAsync(ResetPasswordConfirmRequest request);
        Task<UnifiedResponse> CompleteProfileUpdateAsync(UpdateProfileRequest request);

    }
}
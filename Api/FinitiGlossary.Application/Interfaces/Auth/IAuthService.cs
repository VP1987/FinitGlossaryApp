using FinitiGlossary.Application.DTOs.Response;
using FinitiGlossary.Domain.Entities.Users;

namespace FinitiGlossary.Application.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<UnifiedResponse> RegisterAsync(string username, string email, string password);
        Task<AuthResponse> LoginAsync(string email, string password);
        Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken);
        Task<UnifiedResponse> ResetPasswordRequestAsync(string email);
        Task<UnifiedResponse> ResetPasswordConfirmAsync(string token, string newPassword);
        Task<UnifiedResponse> CompleteProfileUpdateAsync(int userId, string newEmail, string newUsername, string newPassword);

    }
}
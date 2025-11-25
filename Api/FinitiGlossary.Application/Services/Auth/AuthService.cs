using FinitiGlossary.Application.DTOs.Response;
using FinitiGlossary.Application.Interfaces.Auth;
using FinitiGlossary.Application.Interfaces.Repositories.Token;
using FinitiGlossary.Application.Interfaces.Repositories.UserIRepo;
using FinitiGlossary.Domain.Entities.Auth.Token;
using FinitiGlossary.Domain.Entities.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FinitiGlossary.Application.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _users;
        private readonly IRefreshTokenRepository _refreshTokens;
        private readonly IConfiguration _config;
        private readonly IPasswordHasher _hasher;

        public AuthService(
            IUserRepository users,
            IRefreshTokenRepository refreshTokens,
            IPasswordHasher hasher,
            IConfiguration config)
        {
            _users = users;
            _refreshTokens = refreshTokens;
            _hasher = hasher;
            _config = config;
        }

        // ============================================================
        // REGISTER
        // ============================================================
        public async Task<UnifiedResponse> RegisterAsync(string username, string email, string password)
        {
            var exists = await _users.ExistsByEmailAsync(email);
            if (exists)
                throw new InvalidOperationException("User with this email already exists.");

            var passwordHash = _hasher.Hash(password);

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = passwordHash,
                Role = "User",
                IsAdmin = false,
                CreatedAt = DateTime.UtcNow
            };

            await _users.AddAsync(user);

            return new UnifiedResponse(true, "User registered successfully.");
        }

        // ============================================================
        // LOGIN
        // ============================================================
        public async Task<AuthResponse> LoginAsync(string email, string password)
        {
            var user = await _users.GetByEmailAsync(email)
                ?? throw new InvalidOperationException("Invalid email or password.");

            if (!_hasher.Verify(password, user.PasswordHash))
                throw new InvalidOperationException("Invalid email or password.");

            var jwt = GenerateJwtToken(user);
            var refresh = CreateRefreshToken(user.Id);

            await _refreshTokens.AddAsync(refresh);

            var flags = new UserFlags(
                user.MustChangePassword,
                user.MustUpdateProfile
            );

            return new AuthResponse(
                Success: true,
                Token: jwt,
                RefreshToken: refresh.Token,
                Message: "Login successful.",
                Flags: flags
            );
        }

        // ============================================================
        // REFRESH TOKEN
        // ============================================================
        public async Task<RefreshTokenResponse>
            RefreshTokenAsync(string refreshToken)
        {
            var stored = await _refreshTokens.GetValidTokenAsync(refreshToken)
                ?? throw new InvalidOperationException("Invalid or expired refresh token.");

            stored.IsRevoked = true;
            await _refreshTokens.UpdateAsync(stored);

            var newJwt = GenerateJwtToken(stored.User!);
            var newRefresh = CreateRefreshToken(stored.UserId);

            await _refreshTokens.AddAsync(newRefresh);

            return new RefreshTokenResponse(true, newJwt, newRefresh.Token, "Token refreshed.");
        }

        // ============================================================
        // PASSWORD RESET REQUEST
        // ============================================================
        public async Task<UnifiedResponse> ResetPasswordRequestAsync(string email)
        {
            var user = await _users.GetByEmailAsync(email)
                ?? throw new InvalidOperationException("No user found with that email.");

            user.ResetToken = Guid.NewGuid().ToString();
            user.ResetTokenExpires = DateTime.UtcNow.AddHours(1);

            await _users.UpdateAsync(user);

            var resetUrl = $"{_config["Frontend:BaseUrl"]}/reset-password?token={user.ResetToken}";

            var html = $@"
                <h2>Password Reset</h2>
                <p>Click the link below to reset your password:</p>
                <a href=""{resetUrl}"">{resetUrl}</a>
            ";

            // TODO: Implement email service for sending password reset link.

            return new UnifiedResponse(true, "Password reset email has been sent.");
        }

        // ============================================================
        // PASSWORD CONFIRM
        // ============================================================
        public async Task<UnifiedResponse> ResetPasswordConfirmAsync(string token, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new InvalidOperationException("Invalid reset token.");

            var user = await _users.GetByResetTokenAsync(token)
                ?? throw new InvalidOperationException("Invalid or expired reset token.");

            if (user.ResetTokenExpires == null || user.ResetTokenExpires < DateTime.UtcNow)
                throw new InvalidOperationException("Reset token has expired.");

            user.PasswordHash = _hasher.Hash(newPassword);
            user.ResetToken = null;
            user.ResetTokenExpires = null;

            await _users.UpdateAsync(user);

            return new UnifiedResponse(true, "Password has been reset successfully.");
        }

        // ============================================================
        // HELPERS
        // ============================================================
        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim("id", user.Id.ToString()),
                new Claim("username", user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("isAdmin", user.IsAdmin.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private RefreshToken CreateRefreshToken(int userId)
        {
            return new RefreshToken
            {
                Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _users.GetByEmailAsync(email);
        }

        public async Task<UnifiedResponse> CompleteProfileUpdateAsync(
    int userId,
    string newUsername,
    string newEmail,
    string newPassword)
        {
            var user = await _users.GetUserByIdAsync(userId)
                ?? throw new InvalidOperationException("User not found.");

            if (string.IsNullOrWhiteSpace(newUsername))
            {
                throw new InvalidOperationException("Username cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(newEmail))
            {
                throw new InvalidOperationException("Email cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                throw new InvalidOperationException("Password cannot be empty.");
            }

            user.Username = newUsername;
            user.Email = newEmail;
            user.PasswordHash = _hasher.Hash(newPassword);
            user.MustUpdateProfile = false;

            await _users.UpdateAsync(user);

            return new UnifiedResponse(true, "Profile updated successfully.");
        }

    }
}

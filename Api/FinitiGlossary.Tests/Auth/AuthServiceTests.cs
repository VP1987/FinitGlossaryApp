using FinitiGlossary.Application.DTOs.Request;
using FinitiGlossary.Domain.Entities.Auth.Token;
using FinitiGlossary.Domain.Entities.Users;
using Moq;

namespace FinitiGlossary.Tests.Auth
{
    public class AuthServiceTests : AuthServiceTestsBase
    {
        // ============================================================
        // 1. REGISTER
        // ============================================================
        [Fact]
        public async Task RegisterAsync_ShouldThrow_WhenEmailExists()
        {
            _userRepoMock.Setup(x => x.ExistsByEmailAsync("test@test.com"))
                .ReturnsAsync(true);

            var req = new RegisterRequest
            {
                Username = "user",
                Email = "test@test.com",
                Password = "pass"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _authService.RegisterAsync(req));
        }

        [Fact]
        public async Task RegisterAsync_ShouldSuccess_WhenDataValid()
        {
            _userRepoMock.Setup(x => x.ExistsByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            _hasherMock.Setup(x => x.Hash("pass")).Returns("HASHED");

            var req = new RegisterRequest
            {
                Username = "user",
                Email = "test@test.com",
                Password = "pass"
            };

            var result = await _authService.RegisterAsync(req);

            Assert.True(result.Success);
            _userRepoMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        }

        // ============================================================
        // 2. LOGIN
        // ============================================================
        [Fact]
        public async Task LoginAsync_ShouldThrow_WhenUserNotFound()
        {
            _userRepoMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _authService.LoginAsync(new LoginRequest("email@test.com", "pass")));
        }

        [Fact]
        public async Task LoginAsync_ShouldThrow_WhenPasswordWrong()
        {
            var user = new User { Email = "test@test.com", PasswordHash = "HASH" };

            _userRepoMock.Setup(x => x.GetByEmailAsync("test@test.com"))
                .ReturnsAsync(user);

            _hasherMock.Setup(x => x.Verify("WRONG", "HASH")).Returns(false);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _authService.LoginAsync(new LoginRequest("test@test.com", "WRONG")));
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnTokens_WhenValid()
        {
            var user = new User
            {
                Id = 1,
                Email = "ok@test.com",
                Role = "User",
                PasswordHash = "HASH"
            };

            _userRepoMock.Setup(x => x.GetByEmailAsync(user.Email))
                .ReturnsAsync(user);

            _hasherMock.Setup(x => x.Verify("pass", "HASH")).Returns(true);

            var result = await _authService.LoginAsync(new LoginRequest("ok@test.com", "pass"));

            Assert.True(result.Success);
            Assert.False(string.IsNullOrWhiteSpace(result.Token));
            Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
        }

        // ============================================================
        // 3. REFRESH TOKEN
        // ============================================================
        [Fact]
        public async Task RefreshTokenAsync_ShouldThrow_WhenInvalid()
        {
            _refreshRepoMock.Setup(x => x.GetValidTokenAsync("bad"))
                .ReturnsAsync((RefreshToken?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _authService.RefreshTokenAsync("bad"));
        }

        [Fact]
        public async Task RefreshTokenAsync_ShouldRotate_WhenValid()
        {
            var user = new User { Id = 1, Email = "x", Role = "User" };
            var token = new RefreshToken { Token = "old", UserId = 1, User = user };

            _refreshRepoMock.Setup(x => x.GetValidTokenAsync("old"))
                .ReturnsAsync(token);

            var result = await _authService.RefreshTokenAsync("old");

            Assert.True(result.Success);
            Assert.True(token.IsRevoked);
            _refreshRepoMock.Verify(x => x.UpdateAsync(token), Times.Once);
            _refreshRepoMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
        }

        // ============================================================
        // 4. RESET PASSWORD REQUEST (SECURITY-CORRECT)
        // ============================================================
        [Fact]
        public async Task ResetPasswordRequestAsync_ShouldNotThrow_WhenUserMissing()
        {
            _userRepoMock.Setup(x => x.GetByEmailAsync("ghost@test.com"))
                .ReturnsAsync((User?)null);

            var ex = await Record.ExceptionAsync(() =>
                _authService.ResetPasswordRequestAsync("ghost@test.com"));

            Assert.Null(ex);
        }

        [Fact]
        public async Task ResetPasswordRequestAsync_ShouldSetToken_WhenValid()
        {
            var user = new User { Email = "test@test.com" };

            _userRepoMock.Setup(x => x.GetByEmailAsync("test@test.com"))
                .ReturnsAsync(user);

            var result = await _authService.ResetPasswordRequestAsync("test@test.com");

            Assert.True(result.Success);
            Assert.NotNull(user.ResetToken);
            _userRepoMock.Verify(x => x.UpdateAsync(user), Times.Once);
        }

        // ============================================================
        // 5. RESET PASSWORD CONFIRM
        // ============================================================
        [Fact]
        public async Task ResetPasswordConfirmAsync_ShouldThrow_WhenTokenInvalid()
        {
            _userRepoMock.Setup(x => x.GetByResetTokenAsync("bad"))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _authService.ResetPasswordConfirmAsync(
                    new ResetPasswordConfirmRequest("bad", "newpass")));
        }

        [Fact]
        public async Task ResetPasswordConfirmAsync_ShouldThrow_WhenExpired()
        {
            var user = new User
            {
                ResetToken = "expired",
                ResetTokenExpires = DateTime.UtcNow.AddHours(-2)
            };

            _userRepoMock.Setup(x => x.GetByResetTokenAsync("expired"))
                .ReturnsAsync(user);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _authService.ResetPasswordConfirmAsync(
                    new ResetPasswordConfirmRequest("expired", "x")));
        }

        [Fact]
        public async Task ResetPasswordConfirmAsync_ShouldReset_WhenValid()
        {
            var user = new User
            {
                ResetToken = "valid",
                ResetTokenExpires = DateTime.UtcNow.AddHours(1),
                PasswordHash = "OLD"
            };

            _userRepoMock.Setup(x => x.GetByResetTokenAsync("valid"))
                .ReturnsAsync(user);

            _hasherMock.Setup(x => x.Hash("new_pass"))
                .Returns("HASHED");

            var result = await _authService.ResetPasswordConfirmAsync(
                new ResetPasswordConfirmRequest("valid", "new_pass"));

            Assert.True(result.Success);
            Assert.Equal("HASHED", user.PasswordHash);
            Assert.Null(user.ResetToken);
            _userRepoMock.Verify(x => x.UpdateAsync(user), Times.Once);
        }
    }
}

using FinitiGlossary.Domain.Entities.Auth.Token;
using FinitiGlossary.Domain.Entities.Users;
using Moq;

namespace FinitiGlossary.Tests.Auth
{
    public class AuthServiceTests : AuthServiceTestsBase
    {
        // ============================================================
        // 1. REGISTER TESTS
        // ============================================================
        [Fact]
        public async Task RegisterAsync_ShouldThrow_WhenEmailExists()
        {
            _userRepoMock.Setup(x => x.ExistsByEmailAsync("test@test.com")).ReturnsAsync(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _authService.RegisterAsync("user", "test@test.com", "pass"));
        }

        [Fact]
        public async Task RegisterAsync_ShouldSuccess_WhenDataIsValid()
        {
            _userRepoMock.Setup(x => x.ExistsByEmailAsync(It.IsAny<string>())).ReturnsAsync(false);
            _hasherMock.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashed_pass");

            var result = await _authService.RegisterAsync("user", "new@test.com", "pass");

            Assert.True(result.Success);
            _userRepoMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        }

        // ============================================================
        // 2. LOGIN TESTS
        // ============================================================
        [Fact]
        public async Task LoginAsync_ShouldThrow_WhenUserNotFound()
        {
            _userRepoMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _authService.LoginAsync("wrong@test.com", "pass"));
        }

        [Fact]
        public async Task LoginAsync_ShouldThrow_WhenPasswordWrong()
        {
            var user = new User { Email = "test@test.com", PasswordHash = "hash" };
            _userRepoMock.Setup(x => x.GetByEmailAsync(user.Email)).ReturnsAsync(user);
            _hasherMock.Setup(x => x.Verify("wrong", "hash")).Returns(false);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _authService.LoginAsync(user.Email, "wrong"));
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenCredentialsValid()
        {
            var user = new User { Id = 1, Email = "ok@test.com", Role = "User", PasswordHash = "hash" };
            _userRepoMock.Setup(x => x.GetByEmailAsync(user.Email)).ReturnsAsync(user);
            _hasherMock.Setup(x => x.Verify("pass", "hash")).Returns(true);

            var result = await _authService.LoginAsync(user.Email, "pass");

            Assert.True(result.Success);
            Assert.NotEmpty(result.Token);
            Assert.NotEmpty(result.RefreshToken);
        }

        [Fact]
        public async Task LoginAsync_ShouldThrow_OnRepositoryException()
        {
            _userRepoMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("DB ERROR"));

            await Assert.ThrowsAsync<Exception>(() =>
                _authService.LoginAsync("test@test.com", "pass"));
        }

        // ============================================================
        // 3. REFRESH TOKEN TESTS
        // ============================================================
        [Fact]
        public async Task RefreshTokenAsync_ShouldThrow_WhenInvalid()
        {
            _refreshRepoMock.Setup(x => x.GetValidTokenAsync(It.IsAny<string>()))
                .ReturnsAsync((RefreshToken?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _authService.RefreshTokenAsync("bad_token"));
        }

        [Fact]
        public async Task RefreshTokenAsync_ShouldRotateToken_WhenValid()
        {
            var user = new User { Id = 1, Email = "x", Role = "User" };
            var old = new RefreshToken { Token = "old", UserId = 1, User = user };

            _refreshRepoMock.Setup(x => x.GetValidTokenAsync("old")).ReturnsAsync(old);

            var result = await _authService.RefreshTokenAsync("old");

            Assert.True(result.Success);
            Assert.True(old.IsRevoked);
            _refreshRepoMock.Verify(x => x.UpdateAsync(old), Times.Once);
            _refreshRepoMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
        }

        // ============================================================
        // 4. RESET PASSWORD TESTS
        // ============================================================
        [Fact]
        public async Task ResetPasswordRequestAsync_ShouldThrow_WhenUserMissing()
        {
            _userRepoMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _authService.ResetPasswordRequestAsync("ghost@test.com"));
        }

        [Fact]
        public async Task ResetPasswordRequestAsync_ShouldSendEmail_WhenValid()
        {
            var user = new User { Email = "test@test.com" };
            _userRepoMock.Setup(x => x.GetByEmailAsync("test@test.com")).ReturnsAsync(user);

            var result = await _authService.ResetPasswordRequestAsync(user.Email);

            Assert.True(result.Success);
            Assert.NotNull(user.ResetToken);
            _userRepoMock.Verify(x => x.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task ResetPasswordConfirmAsync_ShouldThrow_WhenTokenInvalid()
        {
            _userRepoMock.Setup(x => x.GetByResetTokenAsync("bad_token"))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _authService.ResetPasswordConfirmAsync("bad_token", "newpass"));
        }

        [Fact]
        public async Task ResetPasswordConfirmAsync_ShouldThrow_WhenTokenExpired()
        {
            var user = new User
            {
                ResetToken = "expired",
                ResetTokenExpires = DateTime.UtcNow.AddHours(-2)
            };

            _userRepoMock.Setup(x => x.GetByResetTokenAsync("expired")).ReturnsAsync(user);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _authService.ResetPasswordConfirmAsync("expired", "newpass"));
        }

        [Fact]
        public async Task ResetPasswordConfirmAsync_ShouldReset_WhenValid()
        {
            var user = new User
            {
                ResetToken = "valid",
                ResetTokenExpires = DateTime.UtcNow.AddHours(1),
                PasswordHash = "old"
            };

            _userRepoMock.Setup(x => x.GetByResetTokenAsync("valid")).ReturnsAsync(user);
            _hasherMock.Setup(x => x.Hash("new_pass")).Returns("HASHED");

            var result = await _authService.ResetPasswordConfirmAsync("valid", "new_pass");

            Assert.True(result.Success);
            Assert.Equal("HASHED", user.PasswordHash);
            Assert.Null(user.ResetToken);
            _userRepoMock.Verify(x => x.UpdateAsync(user), Times.Once);
        }
    }
}

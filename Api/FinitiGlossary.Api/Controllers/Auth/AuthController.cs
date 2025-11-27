using FinitiGlossary.Application.DTOs.Request;
using FinitiGlossary.Application.Interfaces.Auth;
using Microsoft.AspNetCore.Mvc;

namespace FinitiGlossary.Api.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var result = await _authService.RegisterAsync(request);

                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }

                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register failed.");
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var result = await _authService.LoginAsync(request);

                if (!result.Success)
                {
                    return Unauthorized(new { message = result.Message });
                }

                return Ok(result
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed.");
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(request.RefreshToken);

                if (!result.Success)
                {
                    return Unauthorized(new { message = result.Message });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Refresh token failed.");
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        [HttpPost("reset-password/request")]
        public async Task<IActionResult> ResetPasswordRequest([FromBody] ResetPasswordRequest request)
        {
            try
            {
                var result = await _authService.ResetPasswordRequestAsync(request.Email);

                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }

                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reset password request failed.");
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        [HttpPost("reset-password/confirm")]
        public async Task<IActionResult> ResetPasswordConfirm([FromBody] ResetPasswordConfirmRequest request)
        {
            try
            {
                var result = await _authService.ResetPasswordConfirmAsync(request);

                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }

                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reset password confirm failed.");
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        [HttpPost("complete-profile-update")]
        public async Task<IActionResult> CompleteProfileUpdate([FromBody] CompleteProfileUpdateRequest request)
        {
            try
            {
                var result = await _authService.CompleteProfileUpdateAsync(new UpdateProfileRequest(
                    request.UserId,
                    request.NewUsername,
                    request.NewEmail,
                    request.NewPassword)
                );

                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }

                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Complete profile update failed.");
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

    }
}

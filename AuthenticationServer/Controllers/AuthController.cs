using AuthenticationServer.DTO;
using AuthenticationServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
namespace AuthenticationServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {

        private readonly IAuthService _authService = authService;

        [Authorize]
        [HttpGet("claims")]
        public IActionResult Claims()
        {
            return Ok(User.Claims.Select(c => new
            {
                c.Type,
                c.Value
            }));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult AdminOnly()
        {
            return Ok("Welcome Admin");
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            return Ok(new
            {
                User = HttpContext.User.Identity?.Name,
                UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Email = User.FindFirst(ClaimTypes.Email)?.Value
            });
        }

        [HttpGet("hello")]
        public IActionResult Hello()
        {
            return Ok("Welcome Chetan! This is our Authentication Server 🚀");
        }

        [HttpPost("login")]
        [EnableRateLimiting("login")]

        public async Task<IActionResult> Login(LoginRequest request)
        {
            var loginResponse = await _authService.LoginAsync(request);
            return Ok(loginResponse);
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup(RegisterRequest request)
        {
            await _authService.RegisterAsync(request);

            return Created();
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshRequest request)
        {
            var response = await _authService.RefreshAsync(request);
            return Ok(response);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(LogoutRequest request)
        {
            await _authService.LogoutAsync(request);
            return Ok();
        }

        [Authorize]
        [HttpPost("logout-all")]
        public async Task<IActionResult> LogoutAll()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = Guid.Parse(userIdClaim);

            await _authService.LogoutFromAllDevices(userId);

            return Ok(new
            {
                message = "Logged out from all devices."
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
        {
            await _authService.ForgotPasswordAsync(request);

            return Ok(new
            {
                message = "If the account exists, a password reset link has been sent."
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(
    ResetPasswordRequest request)
        {
            await _authService.ResetPasswordAsync(request);

            return Ok(new
            {
                message = "Password has been reset successfully."
            });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            // Get user ID from JWT claims
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = Guid.Parse(userIdClaim);

            await _authService.ChangePasswordAsync(
                userId,
                request);

            return Ok(new
            {
                message = "Password changed successfully."
            });
        }


        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(
    VerifyEmailRequest request)
        {
            await _authService.VerifyEmailAsync(request);

            return Ok(new
            {
                message = "Email verified successfully."
            });
        }
    }
}

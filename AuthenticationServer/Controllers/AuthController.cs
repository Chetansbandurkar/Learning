using AuthenticationServer.DTO;
using AuthenticationServer.Services;
using Microsoft.AspNetCore.Mvc;
namespace AuthenticationServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {

        private readonly IAuthService _authService = authService;

        [HttpGet("hello")]
        public IActionResult Hello()
        {
            return Ok("Welcome Chetan! This is our Authentication Server 🚀");
        }

        [HttpPost("login")]
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
    }
}

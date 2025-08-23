using AuthSystem.src.DTOs;
using AuthSystem.src.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthSystem.src.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(AuthService authService) : ControllerBase
    {
        private readonly AuthService _authService = authService;

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto userRegisterDto)
        {
            var user = await _authService.RegisterAsync(userRegisterDto);
            if (user == null)
            {
                return BadRequest("Username or Email already exists.");
            }
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto userLoginDto)
        {
            var token = await _authService.LoginAsync(userLoginDto);
            if (token == null)
            {
                return Unauthorized("Invalid username or password.");
            }
            return Ok(new { token });
        }
    }
}
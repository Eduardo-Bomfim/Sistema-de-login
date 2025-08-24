using System.Security.Claims;
using AuthSystem.src.DTOs;
using AuthSystem.src.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
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
            return Ok(token);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            var responseToken = await _authService.RefreshTokenAsync(refreshToken);
            if (responseToken == null)
            {
                return Unauthorized("Invalid refresh token.");
            }
            return Ok(responseToken);
        }

        [HttpGet("user-data")]
        [Authorize(Roles = "User, Admin")]
        public IActionResult GetUserData()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var claims = identity?.Claims.Select(c => new { type = c.Type, value = c.Value });

            return Ok(claims);
        }

        [HttpGet("admin-data")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAdminData()
        {
            return Ok(new { message = "This is admin data." });
        }
    }
}
using System.Security.Claims;
using AuthSystem.src.DTOs;
using AuthSystem.src.Services;
using Microsoft.AspNetCore.Authorization;
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
                return BadRequest("Invalid username or password.");
            }

            SetRefreshTokenInCookie(token.RefreshToken);
            Response.Cookies.Append("accessToken", token.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMinutes(15)
            });
            return Ok("Login successful. Tokens stored in cookies.");
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized("No refresh token provided.");
            }

            var responseToken = await _authService.RefreshTokenAsync(refreshToken);
            if (responseToken == null)
            {
                return Unauthorized("Invalid refresh token.");
            }

            SetRefreshTokenInCookie(responseToken.RefreshToken);
            Response.Cookies.Append("accessToken", responseToken.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMinutes(15)
            });

            return Ok("Token refreshed. New tokens stored in cookies.");
        }
        
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            await _authService.ForgotPasswordAsync(forgotPasswordDto);
            return Ok(new { message = "If the email is registered, a password reset link has been sent." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            var result = await _authService.ResetPasswordAsync(resetPasswordDto);

            if (!result)
            {
                return BadRequest("Invalid or expired reset token.");
            }

            return Ok("Password has been reset successfully.");
        }

        // Get user data - accessible to both User and Admin roles
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

        // Helper method to set refresh token in HttpOnly cookie
        private void SetRefreshTokenInCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),
                Secure = true,
                SameSite = SameSiteMode.None
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}
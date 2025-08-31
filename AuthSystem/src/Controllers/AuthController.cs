using System.Security.Claims;
using AuthSystem.src.DTOs;
using AuthSystem.src.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthSystem.src.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuth authService) : ControllerBase
    {
        private readonly IAuth _authService = authService;
        
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto userRegisterDto)
        {
            var (user, errorMessage) = await _authService.RegisterAsync(userRegisterDto);
            if (user == null)
            {
                return BadRequest(new { message = errorMessage});
            }
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto userLoginDto)
        {
            var (token, errorMessage) = await _authService.LoginAsync(userLoginDto);
            if (token == null)
            {
                return BadRequest(new { message = errorMessage });
            }

            SetRefreshTokenInCookie(token.RefreshToken);
            Response.Cookies.Append("accessToken", token.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMinutes(15)
            });
            return Ok(new { message = "Login successful. Tokens stored in cookies." });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(new { message = "Refresh token is missing." });
            }

            var responseToken = await _authService.RefreshTokenAsync(refreshToken);
            if (responseToken == null)
            {
                return Unauthorized(new { message = "Invalid refresh token." });
            }

            SetRefreshTokenInCookie(responseToken.RefreshToken);
            Response.Cookies.Append("accessToken", responseToken.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMinutes(15)
            });

            return Ok(new { message = "Token refreshed successfully." });
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
                return BadRequest(new { message = "Invalid or expired token." });
            }

            return Ok(new { message = "Password has been reset successfully." });
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string email, [FromQuery] string token)
        {
            var result = await _authService.ConfirmEmailAsync(email, token);
            if (!result)
            {
                return BadRequest(new { message = "Invalid or expired token." });
            }
            return Ok(new { message = "Email verified successfully." });
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

        [HttpGet("test-error")]
        public IActionResult TestError()
        {
            throw new Exception("Este é um erro de teste para verificar o middleware!");
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
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthSystem.src.Data;
using AuthSystem.src.DTOs;
using AuthSystem.src.Interfaces;
using AuthSystem.src.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuthSystem.src.Services
{
    public class AuthService(AuthDbContext context, IConfiguration configuration, IEmail emailService) : IAuth
    {
        private readonly AuthDbContext _context = context;
        private readonly IConfiguration _configuration = configuration;
        private readonly IEmail _emailService = emailService;
        private const int MaxFailedAccessAttempts = 5;
        private static readonly TimeSpan DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

        public async Task<(LoginResponseDto? response, string? errorMessage)> LoginAsync(UserLoginDto userLoginDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == userLoginDto.Username || u.Email == userLoginDto.Email);

            if (user == null)
            {
                return (null, "Invalid username or password.");
            }
            
            if (!user.IsEmailConfirmed)
            {
                return (null, "Email is not confirmed. Please verify your email before logging in.");
            }

            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            {
                return (null, $"Account is locked. Try again at {user.LockoutEnd.Value:HH:mm} UTC.");
            }

            if (!BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.PasswordHash))
            {
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= MaxFailedAccessAttempts)
                {
                    user.LockoutEnd = DateTime.UtcNow.Add(DefaultLockoutTimeSpan);
                }
                await _context.SaveChangesAsync();
                return (null, "Invalid username or password.");
            }

            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;

            string accessToken = CreateToken(user);
            string refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.TokenCreated = DateTime.UtcNow;
            user.TokenExpires = DateTime.UtcNow.AddDays(7);

            await _context.SaveChangesAsync();

            return (new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            }, null);
        }

        public async Task<(User? user, string? errorMessage)> RegisterAsync(UserRegisterDto userRegisterDto)
        {

            if (!IsPasswordStrong(userRegisterDto.Password))
            {
                return (null, "Password does not meet strength requirements.");
            }

            if (await _context.Users.AnyAsync(u => u.Username == userRegisterDto.Username || u.Email == userRegisterDto.Email))
            {
                return (null, "Username or email already exists.");
            }

            string PasswordHash = BCrypt.Net.BCrypt.HashPassword(userRegisterDto.Password);
            var verificationToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
            var user = new User
            {
                Username = userRegisterDto.Username,
                PasswordHash = PasswordHash,
                Email = userRegisterDto.Email,
                Role = "User", // Default role assignment
                IsEmailConfirmed = false,
                EmailConfirmationToken = verificationToken,
                EmailConfirmationTokenExpires = DateTime.UtcNow.AddMinutes(30)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _emailService.SendEmailConfirmationAsync(user.Email, verificationToken);

            return (user, null);
        }

        public async Task<LoginResponseDto?> RefreshTokenAsync(string refreshToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (user == null || user.TokenExpires < DateTime.UtcNow)
            {
                return null;
            }

            var newAccessToken = CreateToken(user);
            var newRefreshToken = Guid.NewGuid().ToString();

            user.RefreshToken = newRefreshToken;
            user.TokenCreated = DateTime.UtcNow;
            user.TokenExpires = DateTime.UtcNow.AddDays(7);

            await _context.SaveChangesAsync();

            return new LoginResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == resetPasswordDto.Token);

            if (user == null || user.ResetTokenExpires < DateTime.UtcNow)
            {
                return false;
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.NewPassword);
            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == forgotPasswordDto.Email);

            if (user == null)
            {
                return true; // To prevent email enumeration;
            }

            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
            user.PasswordResetToken = token;
            user.ResetTokenExpires = DateTime.UtcNow.AddMinutes(15);

            await _context.SaveChangesAsync();

            await _emailService.SendPasswordResetEmailAsync(user.Email, token);
            return true;
        }

        public async Task<bool> ConfirmEmailAsync(string email, string confirmationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || user.EmailConfirmationToken != confirmationToken || user.IsEmailConfirmed)
            {
                return false;
            }

            user.IsEmailConfirmed = true;
            user.EmailConfirmationToken = null;
            user.EmailConfirmationTokenExpires = null;

            await _context.SaveChangesAsync();

            return true;
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role),
            };

            var appSettingsToken = _configuration.GetSection("AppSettings:Token").Value;

            if (appSettingsToken is null)
            {
                throw new Exception("AppSettings Token is null");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettingsToken));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private static bool IsPasswordStrong(string password)
        {
            if (password.Length < 8) return false;
            if (!password.Any(char.IsUpper)) return false;
            if (!password.Any(char.IsLower)) return false;
            if (!password.Any(char.IsDigit)) return false;
            if (password.All(char.IsLetterOrDigit)) return false;

            return true;
        }

    }
}
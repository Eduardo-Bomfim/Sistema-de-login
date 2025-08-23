using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthSystem.src.Data;
using AuthSystem.src.DTOs;
using AuthSystem.src.Interfaces;
using AuthSystem.src.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuthSystem.src.Services
{
    public class AuthService(AuthDbContext context, IConfiguration configuration) : IAuth
    {
        private readonly AuthDbContext _context = context;
        private readonly IConfiguration _configuration = configuration;

        public async Task<string?> LoginAsync(UserLoginDto userLoginDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == userLoginDto.Username || u.Email == userLoginDto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.PasswordHash))
            {
                return null;
            }

            return CreateToken(user);
        }

        public async Task<User?> RegisterAsync(UserRegisterDto userRegisterDto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == userRegisterDto.Username || u.Email == userRegisterDto.Email))
            {
                return null;
            }

            string PasswordHash = BCrypt.Net.BCrypt.HashPassword(userRegisterDto.Password);

            var user = new User
            {
                Username = userRegisterDto.Username,
                PasswordHash = PasswordHash,
                Email = userRegisterDto.Email
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Email, user.Email),
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
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
using AuthSystem.src.DTOs;
using AuthSystem.src.Models;

namespace AuthSystem.src.Interfaces
{
    public interface IAuth
    {
        Task<User?> RegisterAsync(UserRegisterDto userRegisterDto);
        Task<LoginResponseDto?> LoginAsync(UserLoginDto userLoginDto);
        Task<LoginResponseDto?> RefreshTokenAsync(string refreshToken);
        Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
    }
}

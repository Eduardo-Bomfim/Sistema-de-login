using AuthSystem.src.DTOs;
using AuthSystem.src.Models;

namespace AuthSystem.src.Interfaces
{
    public interface IAuth
    {
        Task<(User? user, string? errorMessage)> RegisterAsync(UserRegisterDto userRegisterDto);
        Task<(LoginResponseDto? response, string? errorMessage)> LoginAsync(UserLoginDto userLoginDto);
        Task<LoginResponseDto?> RefreshTokenAsync(string refreshToken);
        Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
        Task<bool> ConfirmEmailAsync(string email, string confirmationToken);
    }
}

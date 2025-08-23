using AuthSystem.src.DTOs;
using AuthSystem.src.Models;

namespace AuthSystem.src.Interfaces
{
    public interface IAuth
    {
        Task<User?> RegisterAsync(UserRegisterDto userRegisterDto);
        Task<string?> LoginAsync(UserLoginDto userLoginDto);
    }
}

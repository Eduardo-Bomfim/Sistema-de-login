using System.ComponentModel.DataAnnotations;

namespace AuthSystem.src.DTOs
{
    public class ResetPasswordDto
    {
        public required string Token { get; set; } = string.Empty;

        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public required string NewPassword { get; set; } = string.Empty;
    }
}
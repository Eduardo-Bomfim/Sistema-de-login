using System.ComponentModel.DataAnnotations;

namespace AuthSystem.src.DTOs
{
    public class ForgotPasswordDto
    {
        [EmailAddress]
        public required string Email { get; set; }
    }
}
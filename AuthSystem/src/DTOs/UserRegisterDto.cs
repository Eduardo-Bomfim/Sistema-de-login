using AuthSystem.src.Validation;

namespace AuthSystem.src.DTOs
{
    public class UserRegisterDto
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        [StrictEmailAddress]
        public required string Email { get; set; }
    }
}

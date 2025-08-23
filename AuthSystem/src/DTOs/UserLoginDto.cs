namespace AuthSystem.src.DTOs
{
    public class UserLoginDto
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}

namespace AuthSystem.src.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public required string Email { get; set; }

        // Token management properties
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime TokenCreated { get; set; }
        public DateTime TokenExpires { get; set; }

        // Role property for user roles (e.g., Admin, User)
        public string Role { get; set; } = string.Empty;

        // Password reset properties
        public string? PasswordResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }

        // Account lockout properties
        public int FailedLoginAttempts { get; set; }
        public DateTime? LockoutEnd { get; set; }

        // Email confirmation properties
        public bool IsEmailConfirmed { get; set; } = false;
        public string? EmailConfirmationToken { get; set; }
        public DateTime? EmailConfirmationTokenExpires { get; set; }
    }
}

namespace AuthSystem.src.Interfaces
{
    public interface IEmail
    {
        Task SendPasswordResetEmailAsync(string toEmail, string resetToken);
        Task SendEmailConfirmationAsync(string toEmail, string confirmationToken);
    }
}
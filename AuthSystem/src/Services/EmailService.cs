using AuthSystem.src.Interfaces;
using MimeKit;
using MimeKit.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using System.Diagnostics;

namespace AuthSystem.src.Services
{
    public class EmailService(IConfiguration configuration) : IEmail
    {
        private readonly IConfiguration _configuration = configuration;

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse("no-reply@yoursite.com"));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = "Password Reset Request";

                var resetLink = $"https://yourfrontend.com/reset-password?token={resetToken}";
                email.Body = new TextPart(TextFormat.Html)
                {
                    Text = $"<p>You are receiving this email because we received a password reset request for your account.</p><p>Please click the following link to reset your password:</p><a href='{resetLink}'>Reset Password</a><p>If you did not request a password reset, no further action is required.</p>"
                };

                var host = _configuration["Mailtrap:Host"];
                var portStr = _configuration["Mailtrap:Port"];
                var username = _configuration["Mailtrap:Username"];
                var password = _configuration["Mailtrap:Password"];

                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(portStr) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    return;
                }

                var port = int.Parse(portStr);
                
                var smtp = new SmtpClient();
                await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(username, password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                Debug.WriteLine("Email de redefinição enviado com sucesso!");
            }
            catch (Exception ex)
            {

                Debug.WriteLine($"Falha ao enviar email: {ex.Message}");
            }
        }
    }
}
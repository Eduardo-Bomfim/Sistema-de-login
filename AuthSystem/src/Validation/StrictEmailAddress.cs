using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace AuthSystem.src.Validation
{
    public class StrictEmailAddress : ValidationAttribute
    {

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            var emailString = value as string;
            if (emailString == null)
            {
                return new ValidationResult("Invalid email format.");
            }

            if (MailAddress.TryCreate(emailString, out var mailAddress))
            {
                var hostParts = mailAddress.Host.Split('.');
                if (hostParts.Length > 1 && !string.IsNullOrEmpty(hostParts.Last()))
                {
                    return ValidationResult.Success;
                }
            }
            return new ValidationResult(ErrorMessage ?? "Invalid email format.");
        }
    }
}
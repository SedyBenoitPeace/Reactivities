using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Infrastructure.Email
{
    public class EmailSender
    {
        private readonly IConfiguration _config;
        public EmailSender(IConfiguration config)
        {
            this._config = config;

        }

        public async Task SendEmailAsync(string userEmail, string emailSubject, string message)
        {
            var client = new SendGridClient(_config["SendGrid:Key"]);

            var email = new SendGridMessage
            {
                From = new EmailAddress("reactivitiessbp@gmail.com", _config["SendGrid:User"]),
                Subject = emailSubject,
                PlainTextContent = message,
                HtmlContent = message
            };

            email.AddTo(new EmailAddress(userEmail));

            await client.SendEmailAsync(email);
        }
    }
}
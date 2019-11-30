using Microsoft.AspNetCore.Identity.UI.Services;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace HAS.Registration.Feature.SendGrid
{
    public class SendGridEmailSender : IEmailSender
    {
        public SendGridEmailSender(AuthMessageSenderOptions options)
        {
            Options = options;
        }

        public AuthMessageSenderOptions Options { get; }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Execute(Options.SendGridKey, subject, htmlMessage, email);
        }

        public Task Execute(string apiKey, string subject, string message, string email)
        {
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("donotreply@mypractice.yoga", "Do Not Reply"),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(email));

            // Disable click tracking
            msg.SetClickTracking(false, false);

            return client.SendEmailAsync(msg);
        }
    }
}

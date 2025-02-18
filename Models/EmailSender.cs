using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace WebApplication8.Models
{
    public class EmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var fromMail = "tahajana057@gmail.com";
            var fromPassword = "iacf hxbt saml xqkm";

            var message = new MailMessage();
            message.From = new MailAddress(fromMail);
            message.Subject = subject;
            message.Body = htmlMessage;
            message.To.Add(email);
            message.IsBodyHtml = true;

            var stmpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromMail, fromPassword),
                EnableSsl = true
            };

            stmpClient.Send(message);

        }
    }
}

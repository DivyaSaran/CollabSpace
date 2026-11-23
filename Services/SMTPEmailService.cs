using System.Net;
using System.Net.Mail;

namespace CollabSpace.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public SmtpEmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var smtpServer = _config["EmailSettings:SmtpServer"]!;
            var port = int.Parse(_config["EmailSettings:Port"] ?? "587");
            var senderName = _config["EmailSettings:SenderName"] ?? "CollabSpace";
            var senderEmail = _config["EmailSettings:SenderEmail"]!;
            var password = _config["EmailSettings:Password"]!;

            using var client = new SmtpClient(smtpServer, port)
            {
                Credentials = new NetworkCredential(senderEmail, password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
        }
    }
}
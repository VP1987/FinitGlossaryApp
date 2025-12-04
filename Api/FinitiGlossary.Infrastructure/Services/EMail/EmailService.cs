using FinitiGlossary.Application.DTOs.Request;
using FinitiGlossary.Application.Interfaces.Auth.EMail;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace FinitiGlossary.Infrastructure.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendAsync(EmailMessageRequest request)
        {
            var host = _config["Email:Host"];
            var port = int.Parse(_config["Email:Port"]);
            var username = _config["Email:Username"];
            var password = _config["Email:Password"];
            var from = _config["Email:From"];

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(username, password)
            };

            var msg = new MailMessage(from, request.To, request.Subject, request.Body) { IsBodyHtml = true };
            await client.SendMailAsync(msg);
        }
    }
}

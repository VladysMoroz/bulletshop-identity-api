using Api.DTOs.Account;
using Mailjet.Client;
using Mailjet.Client.TransactionalEmails;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Net;
using System;
using System.Threading.Tasks;

namespace Api.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }
        
        public async Task<bool> SendEmailAsync(EmailSendDto emailSend)
        {
            try
            {
                var client = new SmtpClient("smtp-mail.outlook.com", 587)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(_config["SMTP:Username"], _config["SMTP:Password"])
                };

                var message = new MailMessage(from: _config["SMTP:Username"],
                    to: emailSend.To, subject: emailSend.Subject, body: emailSend.Body);

                message.IsBodyHtml = true;
                await client.SendMailAsync(message);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

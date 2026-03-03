using Azure.Communication.Email;
using Microsoft.Extensions.Configuration;
using NotificationService.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Services
{
    public sealed class AcsEmailSender : IEmailSender
    {
        private readonly EmailClient _client;
        private readonly string _from;

        public AcsEmailSender(EmailClient client, IConfiguration config)
        {
            _client = client;
            _from = config["AcsEmail:From"] ?? throw new Exception("Missing AcsEmail:From");
        }

        public async Task SendAsync(string toEmail, string subject, string body, CancellationToken ct)
        {
            var content = new EmailContent(subject) { PlainText = body };
            var recipients = new EmailRecipients(new[] { new EmailAddress(toEmail) });
            var email = new EmailMessage(_from, recipients, content);

            await _client.SendAsync(Azure.WaitUntil.Started, email, ct);
        }
    }
}

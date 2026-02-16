using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MimeKit;
using NotificationService.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Services
{
    public class NotificationService : INotificationService
    {
        private readonly FeedbackDbContext _feedbackContext;
        private readonly UserDbContext _userContext;
        private readonly IConfiguration _configuration;

        public NotificationService(FeedbackDbContext feedbackContext, UserDbContext userContext, IConfiguration config)
        {
            _feedbackContext = feedbackContext;
            _userContext = userContext;
            _configuration = config;
        }

        public async Task SendFeedbackNotificationsAsync()
        {
            var feedbacks = await _feedbackContext.Feedbacks
                            .Where(f => f.CreatedAt > DateTime.UtcNow.AddMinutes(-5))
                            .ToListAsync();

            foreach (var feedback in feedbacks)
            {
                var user = await _userContext.Users.FirstOrDefaultAsync(u => u.Id == feedback.UserId);
                if (user == null) continue;

                var subject = "You've received new feedback!";
                var body = $"""
                                Hello {user.FirstName},

                                You received a new feedback:

                                Rating: {feedback.Rating}
                                Comment: {feedback.Comment}

                                Best regards,
                                Feedback Team
                            """;

                await SendEmailAsync(user.Email, subject, body);
            }
        }

        //public async Task SendEmailAsync(string toEmail, string subject, string body)
        //{
        //    var message = new MimeMessage();
        //    message.From.Add(new MailboxAddress("Feedback Platform", "yourEmail@gmail.com")); // Emailul tău Gmail
        //    message.To.Add(new MailboxAddress("", toEmail));
        //    message.Subject = subject;

        //    message.Body = new TextPart("plain")
        //    {
        //        Text = body
        //    };

        //    using var client = new SmtpClient();
        //    await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
        //    await client.AuthenticateAsync("yourEmail@gmail.com", "yourAppPassword"); // aici pui App Password-ul, nu parola contului
        //    await client.SendAsync(message);
        //    await client.DisconnectAsync(true);
        //}

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Feedback Platform", "no-reply@ethereal.email")); // poate fi orice "from"
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            message.Body = new TextPart("plain")
            {
                Text = body
            };

            using var client = new SmtpClient();

            var smtpHost = _configuration["SmtpHost"];
            var smtpPort = int.Parse(_configuration["SmtpPort"]);
            var smtpUser = _configuration["SmtpUser"];
            var smtpPass = _configuration["SmtpPass"];

            await client.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(smtpUser, smtpPass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}

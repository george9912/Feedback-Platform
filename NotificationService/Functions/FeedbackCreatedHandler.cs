using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NotificationService.Clients;
using NotificationService.Common.Events;
using NotificationService.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NotificationService.Functions
{
    public sealed class FeedbackCreatedHandler
    {
        private readonly ILogger _logger;
        private readonly IUserClient _userClient;
        private readonly IEmailSender _emailSender;

        private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

        public FeedbackCreatedHandler(
            ILoggerFactory loggerFactory,
            IUserClient userClient,
            IEmailSender emailSender)
        {
            _logger = loggerFactory.CreateLogger<FeedbackCreatedHandler>();
            _userClient = userClient;
            _emailSender = emailSender;
        }

        [Function("FeedbackCreatedHandler")]
        public async Task Run(
            [ServiceBusTrigger("feedback-created", Connection = "ServiceBusConnection")]
        string message,
            CancellationToken ct)
        {
            FeedbackCreatedEvent evt;
            try
            {
                evt = JsonSerializer.Deserialize<FeedbackCreatedEvent>(message, JsonOpts)
                      ?? throw new Exception("Empty payload");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Invalid message payload. Raw={Raw}", message);
                throw; // retry + DLQ
            }

            // call UserService: GET /api/users/{id}
            var user = await _userClient.GetUserAsync(evt.UserId, ct);
            if (user is null || string.IsNullOrWhiteSpace(user.Email))
            {
                _logger.LogWarning("User not found or missing email. UserId={UserId}", evt.UserId);
                return; // nu retry la infinit pentru not found
            }

            var subject = "Ai primit feedback nou!";
            var body = $"""
        Salut {user.FirstName ?? "there"},

        Ai primit un feedback nou:

        Rating: {evt.Rating}
        Comentariu: {evt.Comment}

        Feedback Platform
        """;

            await _emailSender.SendAsync(user.Email, subject, body, ct);

            _logger.LogInformation("Email requested via ACS. FeedbackId={FeedbackId} -> {Email}",
                evt.FeedbackId, user.Email);
        }
    }
}

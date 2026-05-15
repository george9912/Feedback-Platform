using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NotificationService.Clients;
using NotificationService.Common.Events;
using NotificationService.Interfaces;
using System.Text.Json;

namespace NotificationService.Functions;

public class CampaignNotificationHandler
{
    private readonly ILogger _logger;
    private readonly IUserClient _userClient;
    private readonly IEmailSender _emailSender;

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public CampaignNotificationHandler(
        ILoggerFactory loggerFactory,
        IUserClient userClient,
        IEmailSender emailSender)
    {
        _logger = loggerFactory.CreateLogger<CampaignNotificationHandler>();
        _userClient = userClient;
        _emailSender = emailSender;
    }

    [Function("CampaignNotificationHandler")]
    public async Task Run(
        [ServiceBusTrigger("campaign-notifications", Connection = "ServiceBusConnection")] string message,
        CancellationToken ct)
    {
        CampaignNotificationEvent evt;
        try
        {
            evt = JsonSerializer.Deserialize<CampaignNotificationEvent>(message, JsonOpts)
                ?? throw new Exception("Empty payload");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Invalid campaign notification payload. Raw={Raw}", message);
            throw;
        }

        if (evt.RecipientUserIds.Length == 0)
        {
            _logger.LogInformation("No recipients for campaign notification {CampaignId} {Type}", evt.CampaignId, evt.NotificationType);
            return;
        }

        var subject = evt.NotificationType switch
        {
            "CampaignStart" => $"Campaign started: {evt.CampaignName}",
            "CampaignDeadlineApproaching" => $"Campaign deadline approaching: {evt.CampaignName}",
            "CampaignLastDay" => $"Last day for campaign: {evt.CampaignName}",
            "CampaignCompleted" => $"Campaign completed: {evt.CampaignName}",
            _ => $"Campaign update: {evt.CampaignName}"
        };

        var body = evt.NotificationType switch
        {
            "CampaignStart" => $"The campaign '{evt.CampaignName}' has started. Please submit your feedback.",
            "CampaignDeadlineApproaching" => $"The campaign '{evt.CampaignName}' is approaching its deadline. Please complete pending submissions.",
            "CampaignLastDay" => $"Today is the last day for campaign '{evt.CampaignName}'.",
            "CampaignCompleted" => $"Campaign '{evt.CampaignName}' is now closed. Thank you for participating.",
            _ => $"Campaign '{evt.CampaignName}' has a new update."
        };

        foreach (var userId in evt.RecipientUserIds.Distinct())
        {
            var user = await _userClient.GetUserAsync(userId, ct);
            if (user is null || string.IsNullOrWhiteSpace(user.Email))
            {
                _logger.LogWarning("Could not resolve email for user {UserId}", userId);
                continue;
            }

            await _emailSender.SendAsync(user.Email, subject, body, ct);
        }

        _logger.LogInformation("Campaign notification sent. CampaignId={CampaignId} Type={Type} Recipients={Count}", evt.CampaignId, evt.NotificationType, evt.RecipientUserIds.Length);
    }
}

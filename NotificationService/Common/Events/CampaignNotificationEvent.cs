namespace NotificationService.Common.Events;

public class CampaignNotificationEvent
{
    public Guid CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
    public DateTime TriggeredAtUtc { get; set; }
    public Guid[] RecipientUserIds { get; set; } = [];
}

namespace FeedbackService.API.Features.Feedback.Campaign;

public class CampaignFinalReport
{
    public Guid Id { get; private set; }
    public Guid CampaignId { get; private set; }
    public int TotalParticipants { get; private set; }
    public int CompletedParticipants { get; private set; }
    public int InProgressParticipants { get; private set; }
    public int NotStartedParticipants { get; private set; }
    public int TotalSubmissions { get; private set; }
    public DateTime GeneratedAtUtc { get; private set; }

    private CampaignFinalReport()
    {
    }

    public CampaignFinalReport(
        Guid campaignId,
        int totalParticipants,
        int completedParticipants,
        int inProgressParticipants,
        int notStartedParticipants,
        int totalSubmissions)
    {
        Id = Guid.NewGuid();
        CampaignId = campaignId;
        TotalParticipants = totalParticipants;
        CompletedParticipants = completedParticipants;
        InProgressParticipants = inProgressParticipants;
        NotStartedParticipants = notStartedParticipants;
        TotalSubmissions = totalSubmissions;
        GeneratedAtUtc = DateTime.UtcNow;
    }
}

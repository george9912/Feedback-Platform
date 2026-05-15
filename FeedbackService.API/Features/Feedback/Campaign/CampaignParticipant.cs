namespace FeedbackService.API.Features.Feedback.Campaign;

public class CampaignParticipant
{
    public Guid Id { get; private set; }
    public Guid CampaignId { get; private set; }
    public Guid UserId { get; private set; }
    public int RequiredSubmissions { get; private set; }
    public int SubmittedSubmissions { get; private set; }
    public DateTime InvitedAtUtc { get; private set; }
    public DateTime? LastSubmissionAtUtc { get; private set; }

    private CampaignParticipant()
    {
    }

    public CampaignParticipant(Guid campaignId, Guid userId, int requiredSubmissions)
    {
        if (requiredSubmissions < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(requiredSubmissions), "Required submissions must be at least 1.");
        }

        Id = Guid.NewGuid();
        CampaignId = campaignId;
        UserId = userId;
        RequiredSubmissions = requiredSubmissions;
        SubmittedSubmissions = 0;
        InvitedAtUtc = DateTime.UtcNow;
    }

    public ProgressStatus GetStatus()
    {
        if (SubmittedSubmissions <= 0)
        {
            return ProgressStatus.NotStarted;
        }

        if (SubmittedSubmissions >= RequiredSubmissions)
        {
            return ProgressStatus.Completed;
        }

        return ProgressStatus.InProgress;
    }

    public void RecordSubmission()
    {
        SubmittedSubmissions += 1;
        LastSubmissionAtUtc = DateTime.UtcNow;
    }
}

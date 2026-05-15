namespace FeedbackService.API.Features.Feedback.Campaign;

public class FeedbackCampaign
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime StartDateUtc { get; private set; }
    public DateTime EndDateUtc { get; private set; }
    public CampaignStatus Status { get; private set; }
    public int MinimumRequiredSubmissions { get; private set; }
    public bool IsAnonymous { get; private set; }
    public Guid CreatedByAdminId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public DateTime? ClosedAtUtc { get; private set; }

    private readonly List<CampaignAudienceRule> _audienceRules = [];
    public IReadOnlyCollection<CampaignAudienceRule> AudienceRules => _audienceRules;

    private readonly List<CampaignParticipant> _participants = [];
    public IReadOnlyCollection<CampaignParticipant> Participants => _participants;

    private FeedbackCampaign()
    {
    }

    public FeedbackCampaign(
        string name,
        string description,
        DateTime startDateUtc,
        DateTime endDateUtc,
        int minimumRequiredSubmissions,
        bool isAnonymous,
        Guid createdByAdminId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Campaign name is required.", nameof(name));
        }

        if (endDateUtc < startDateUtc)
        {
            throw new ArgumentException("End date must be after start date.", nameof(endDateUtc));
        }

        if (minimumRequiredSubmissions < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumRequiredSubmissions), "Minimum required submissions must be at least 1.");
        }

        Id = Guid.NewGuid();
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        StartDateUtc = startDateUtc;
        EndDateUtc = endDateUtc;
        MinimumRequiredSubmissions = minimumRequiredSubmissions;
        IsAnonymous = isAnonymous;
        CreatedByAdminId = createdByAdminId;
        Status = CampaignStatus.Draft;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void Update(
        string name,
        string description,
        DateTime startDateUtc,
        DateTime endDateUtc,
        int minimumRequiredSubmissions,
        bool isAnonymous)
    {
        if (Status != CampaignStatus.Draft)
        {
            throw new InvalidOperationException("Only draft campaigns can be edited.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Campaign name is required.", nameof(name));
        }

        if (endDateUtc < startDateUtc)
        {
            throw new ArgumentException("End date must be after start date.", nameof(endDateUtc));
        }

        if (minimumRequiredSubmissions < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumRequiredSubmissions), "Minimum required submissions must be at least 1.");
        }

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        StartDateUtc = startDateUtc;
        EndDateUtc = endDateUtc;
        MinimumRequiredSubmissions = minimumRequiredSubmissions;
        IsAnonymous = isAnonymous;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (Status != CampaignStatus.Draft)
        {
            throw new InvalidOperationException("Only draft campaigns can be activated.");
        }

        if (_participants.Count == 0)
        {
            throw new InvalidOperationException("Cannot activate campaign without participants.");
        }

        Status = CampaignStatus.Active;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Close()
    {
        if (Status == CampaignStatus.Closed)
        {
            return;
        }

        Status = CampaignStatus.Closed;
        ClosedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void ReplaceAudienceRules(IEnumerable<CampaignAudienceRule> rules)
    {
        _audienceRules.Clear();
        _audienceRules.AddRange(rules);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void ReplaceParticipants(IEnumerable<Guid> participantUserIds)
    {
        _participants.Clear();

        foreach (var userId in participantUserIds.Distinct())
        {
            _participants.Add(new CampaignParticipant(Id, userId, MinimumRequiredSubmissions));
        }

        UpdatedAtUtc = DateTime.UtcNow;
    }
}

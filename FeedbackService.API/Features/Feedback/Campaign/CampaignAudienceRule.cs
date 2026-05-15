namespace FeedbackService.API.Features.Feedback.Campaign;

public class CampaignAudienceRule
{
    public Guid Id { get; private set; }
    public Guid CampaignId { get; private set; }
    public TargetAudienceType AudienceType { get; private set; }
    public string RuleValue { get; private set; } = string.Empty;

    private CampaignAudienceRule()
    {
    }

    public CampaignAudienceRule(Guid campaignId, TargetAudienceType audienceType, string ruleValue)
    {
        if (string.IsNullOrWhiteSpace(ruleValue))
        {
            throw new ArgumentException("Rule value is required.", nameof(ruleValue));
        }

        Id = Guid.NewGuid();
        CampaignId = campaignId;
        AudienceType = audienceType;
        RuleValue = ruleValue.Trim();
    }
}

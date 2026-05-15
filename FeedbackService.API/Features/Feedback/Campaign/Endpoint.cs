using FeedbackService.API.Common.Events;
using FeedbackService.API.Common.Notifications;
using FeedbackService.API.Features.Feedback;
using FeedbackService.API.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FeedbackService.API.Features.Feedback.Campaign;

public static class Endpoint
{
    public static IEndpointRouteBuilder MapCampaignRoutes(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/campaigns", CreateCampaign);
        app.MapPut("/api/campaigns/{campaignId:guid}", UpdateCampaign);
        app.MapPost("/api/campaigns/{campaignId:guid}/activate", ActivateCampaign);
        app.MapPost("/api/campaigns/{campaignId:guid}/close", CloseCampaign);
        app.MapGet("/api/campaigns/{campaignId:guid}", GetCampaignById);
        app.MapGet("/api/campaigns", ListCampaigns);
        app.MapGet("/api/campaigns/active/{userId:guid}", ListActiveCampaignsForUser);
        app.MapGet("/api/campaigns/{campaignId:guid}/progress/{userId:guid}", GetUserProgress);
        app.MapGet("/api/campaigns/{campaignId:guid}/progress", GetOverallProgress);
        app.MapGet("/api/campaigns/{campaignId:guid}/report", GetFinalReport);
        app.MapPost("/api/campaigns/{campaignId:guid}/feedback", SubmitCampaignFeedback);

        return app;
    }

    private static async Task<IResult> CreateCampaign(
        CreateCampaignRequest request,
        AppDbContext db,
        CancellationToken ct)
    {
        if (request.EndDateUtc < request.StartDateUtc)
        {
            return Results.BadRequest(new { message = "EndDateUtc must be after StartDateUtc." });
        }

        if (request.MinimumRequiredSubmissions < 1)
        {
            return Results.BadRequest(new { message = "MinimumRequiredSubmissions must be at least 1." });
        }

        if (request.ResolvedParticipantUserIds is null || request.ResolvedParticipantUserIds.Count == 0)
        {
            return Results.BadRequest(new { message = "At least one participant is required." });
        }

        var campaign = new FeedbackCampaign(
            request.Name,
            request.Description,
            request.StartDateUtc,
            request.EndDateUtc,
            request.MinimumRequiredSubmissions,
            request.IsAnonymous,
            request.CreatedByAdminId);

        campaign.ReplaceAudienceRules(MapAudienceRules(campaign.Id, request.AudienceTargets));
        campaign.ReplaceParticipants(request.ResolvedParticipantUserIds);

        db.Campaigns.Add(campaign);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/campaigns/{campaign.Id}", new { campaignId = campaign.Id });
    }

    private static async Task<IResult> UpdateCampaign(
        Guid campaignId,
        UpdateCampaignRequest request,
        AppDbContext db,
        CancellationToken ct)
    {
        var campaign = await db.Campaigns
            .Include(c => c.AudienceRules)
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == campaignId, ct);

        if (campaign is null)
        {
            return Results.NotFound();
        }

        try
        {
            campaign.Update(
                request.Name,
                request.Description,
                request.StartDateUtc,
                request.EndDateUtc,
                request.MinimumRequiredSubmissions,
                request.IsAnonymous);

            campaign.ReplaceAudienceRules(MapAudienceRules(campaign.Id, request.AudienceTargets));

            if (request.ResolvedParticipantUserIds is not null && request.ResolvedParticipantUserIds.Count > 0)
            {
                campaign.ReplaceParticipants(request.ResolvedParticipantUserIds);
            }

            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> ActivateCampaign(
        Guid campaignId,
        AppDbContext db,
        ICampaignNotificationDispatcher dispatcher,
        CancellationToken ct)
    {
        var campaign = await db.Campaigns
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == campaignId, ct);

        if (campaign is null)
        {
            return Results.NotFound();
        }

        try
        {
            campaign.Activate();
            await db.SaveChangesAsync(ct);

            var recipientUserIds = campaign.Participants.Select(p => p.UserId).ToArray();
            var now = DateTime.UtcNow;

            await dispatcher.DispatchAsync(new CampaignNotificationEvent
            {
                CampaignId = campaign.Id,
                CampaignName = campaign.Name,
                NotificationType = "CampaignStart",
                TriggeredAtUtc = now,
                RecipientUserIds = recipientUserIds
            }, campaign.StartDateUtc > now ? campaign.StartDateUtc : now, ct);

            var approachingDeadlineAt = campaign.EndDateUtc.AddDays(-2);
            if (approachingDeadlineAt > now)
            {
                await dispatcher.DispatchAsync(new CampaignNotificationEvent
                {
                    CampaignId = campaign.Id,
                    CampaignName = campaign.Name,
                    NotificationType = "CampaignDeadlineApproaching",
                    TriggeredAtUtc = now,
                    RecipientUserIds = recipientUserIds
                }, approachingDeadlineAt, ct);
            }

            var lastDayAt = campaign.EndDateUtc.AddDays(-1);
            if (lastDayAt > now)
            {
                await dispatcher.DispatchAsync(new CampaignNotificationEvent
                {
                    CampaignId = campaign.Id,
                    CampaignName = campaign.Name,
                    NotificationType = "CampaignLastDay",
                    TriggeredAtUtc = now,
                    RecipientUserIds = recipientUserIds
                }, lastDayAt, ct);
            }

            return Results.Ok(new { message = "Campaign activated." });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> CloseCampaign(
        Guid campaignId,
        AppDbContext db,
        ICampaignNotificationDispatcher dispatcher,
        CancellationToken ct)
    {
        var campaign = await db.Campaigns
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == campaignId, ct);

        if (campaign is null)
        {
            return Results.NotFound();
        }

        campaign.Close();

        var completed = campaign.Participants.Count(p => p.GetStatus() == ProgressStatus.Completed);
        var inProgress = campaign.Participants.Count(p => p.GetStatus() == ProgressStatus.InProgress);
        var notStarted = campaign.Participants.Count(p => p.GetStatus() == ProgressStatus.NotStarted);
        var totalSubmissions = campaign.Participants.Sum(p => p.SubmittedSubmissions);

        var existingReport = await db.CampaignFinalReports.FirstOrDefaultAsync(r => r.CampaignId == campaign.Id, ct);
        if (existingReport is not null)
        {
            db.CampaignFinalReports.Remove(existingReport);
        }

        var report = new CampaignFinalReport(
            campaign.Id,
            campaign.Participants.Count,
            completed,
            inProgress,
            notStarted,
            totalSubmissions);

        db.CampaignFinalReports.Add(report);
        await db.SaveChangesAsync(ct);

        await dispatcher.DispatchAsync(new CampaignNotificationEvent
        {
            CampaignId = campaign.Id,
            CampaignName = campaign.Name,
            NotificationType = "CampaignCompleted",
            TriggeredAtUtc = DateTime.UtcNow,
            RecipientUserIds = campaign.Participants.Select(p => p.UserId).ToArray()
        }, null, ct);

        return Results.Ok(new { message = "Campaign closed." });
    }

    private static async Task<IResult> GetCampaignById(Guid campaignId, AppDbContext db, CancellationToken ct)
    {
        var campaign = await db.Campaigns
            .Include(c => c.AudienceRules)
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == campaignId, ct);

        if (campaign is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(ToCampaignDetails(campaign));
    }

    private static async Task<IResult> ListCampaigns(string? status, AppDbContext db, CancellationToken ct)
    {
        var query = db.Campaigns.AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<CampaignStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(c => c.Status == parsedStatus);
        }

        var campaigns = await query
            .OrderByDescending(c => c.CreatedAtUtc)
            .Select(c => new CampaignListItem(
                c.Id,
                c.Name,
                c.Description,
                c.StartDateUtc,
                c.EndDateUtc,
                c.Status.ToString(),
                c.MinimumRequiredSubmissions,
                c.IsAnonymous,
                c.CreatedAtUtc))
            .ToListAsync(ct);

        return Results.Ok(campaigns);
    }

    private static async Task<IResult> ListActiveCampaignsForUser(Guid userId, AppDbContext db, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var campaigns = await db.Campaigns
            .Include(c => c.Participants)
            .Where(c => c.Status == CampaignStatus.Active
                && c.StartDateUtc <= now
                && c.EndDateUtc >= now
                && c.Participants.Any(p => p.UserId == userId))
            .OrderBy(c => c.EndDateUtc)
            .Select(c => new CampaignListItem(
                c.Id,
                c.Name,
                c.Description,
                c.StartDateUtc,
                c.EndDateUtc,
                c.Status.ToString(),
                c.MinimumRequiredSubmissions,
                c.IsAnonymous,
                c.CreatedAtUtc))
            .ToListAsync(ct);

        return Results.Ok(campaigns);
    }

    private static async Task<IResult> GetUserProgress(Guid campaignId, Guid userId, AppDbContext db, CancellationToken ct)
    {
        var participant = await db.CampaignParticipants
            .FirstOrDefaultAsync(p => p.CampaignId == campaignId && p.UserId == userId, ct);

        if (participant is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(new CampaignUserProgressResponse(
            participant.UserId,
            participant.RequiredSubmissions,
            participant.SubmittedSubmissions,
            participant.GetStatus().ToString(),
            participant.LastSubmissionAtUtc));
    }

    private static async Task<IResult> GetOverallProgress(Guid campaignId, AppDbContext db, CancellationToken ct)
    {
        var participants = await db.CampaignParticipants
            .Where(p => p.CampaignId == campaignId)
            .ToListAsync(ct);

        if (participants.Count == 0)
        {
            return Results.Ok(new CampaignOverallProgressResponse(0, 0, 0, 0, 0));
        }

        var completed = participants.Count(p => p.GetStatus() == ProgressStatus.Completed);
        var inProgress = participants.Count(p => p.GetStatus() == ProgressStatus.InProgress);
        var notStarted = participants.Count(p => p.GetStatus() == ProgressStatus.NotStarted);
        var totalSubmissions = participants.Sum(p => p.SubmittedSubmissions);

        return Results.Ok(new CampaignOverallProgressResponse(
            participants.Count,
            completed,
            inProgress,
            notStarted,
            totalSubmissions));
    }

    private static async Task<IResult> GetFinalReport(Guid campaignId, AppDbContext db, CancellationToken ct)
    {
        var report = await db.CampaignFinalReports.FirstOrDefaultAsync(r => r.CampaignId == campaignId, ct);

        if (report is null)
        {
            return Results.NotFound(new { message = "Final report not available yet." });
        }

        return Results.Ok(report);
    }

    private static async Task<IResult> SubmitCampaignFeedback(
        Guid campaignId,
        SubmitCampaignFeedbackRequest request,
        AppDbContext db,
        CancellationToken ct)
    {
        var campaign = await db.Campaigns
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == campaignId, ct);

        if (campaign is null)
        {
            return Results.NotFound(new { message = "Campaign not found." });
        }

        if (campaign.Status != CampaignStatus.Active)
        {
            return Results.BadRequest(new { message = "Campaign is not active." });
        }

        var now = DateTime.UtcNow;
        if (now < campaign.StartDateUtc || now > campaign.EndDateUtc)
        {
            return Results.BadRequest(new { message = "Campaign is outside submission window." });
        }

        var submitter = campaign.Participants.FirstOrDefault(p => p.UserId == request.SubmittedByUserId);
        if (submitter is null)
        {
            return Results.BadRequest(new { message = "Submitting user is not part of this campaign." });
        }

        var recipientExists = campaign.Participants.Any(p => p.UserId == request.RecipientUserId);
        if (!recipientExists)
        {
            return Results.BadRequest(new { message = "Recipient user is not part of this campaign." });
        }

        var visibility = Enum.TryParse<FeedbackVisibility>(request.Visibility, true, out var parsedVisibility)
            ? parsedVisibility
            : FeedbackVisibility.Public;

        var feedback = new Feedback(
            request.RecipientUserId,
            request.Rating,
            request.Comment,
            visibility,
            request.Tags,
            request.SubmittedByUserId,
            campaignId);

        submitter.RecordSubmission();
        db.Feedbacks.Add(feedback);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/feedback/{feedback.Id}", new { feedbackId = feedback.Id });
    }

    private static List<CampaignAudienceRule> MapAudienceRules(Guid campaignId, List<AudienceTargetDto> audienceTargets)
    {
        var rules = new List<CampaignAudienceRule>();

        foreach (var target in audienceTargets)
        {
            if (!Enum.TryParse<TargetAudienceType>(target.Type, true, out var audienceType))
            {
                continue;
            }

            if (target.Values is null || target.Values.Count == 0)
            {
                rules.Add(new CampaignAudienceRule(campaignId, audienceType, "ALL"));
                continue;
            }

            foreach (var value in target.Values.Where(v => !string.IsNullOrWhiteSpace(v)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                rules.Add(new CampaignAudienceRule(campaignId, audienceType, value));
            }
        }

        return rules;
    }

    private static CampaignDetailsResponse ToCampaignDetails(FeedbackCampaign campaign)
    {
        var completed = campaign.Participants.Count(p => p.GetStatus() == ProgressStatus.Completed);
        var inProgress = campaign.Participants.Count(p => p.GetStatus() == ProgressStatus.InProgress);
        var notStarted = campaign.Participants.Count(p => p.GetStatus() == ProgressStatus.NotStarted);

        return new CampaignDetailsResponse(
            campaign.Id,
            campaign.Name,
            campaign.Description,
            campaign.StartDateUtc,
            campaign.EndDateUtc,
            campaign.Status.ToString(),
            campaign.MinimumRequiredSubmissions,
            campaign.IsAnonymous,
            campaign.CreatedByAdminId,
            campaign.CreatedAtUtc,
            campaign.UpdatedAtUtc,
            campaign.ClosedAtUtc,
            campaign.AudienceRules.Select(r => new AudienceTargetDto(r.AudienceType.ToString(), new List<string> { r.RuleValue })).ToList(),
            campaign.Participants.Select(p => p.UserId).ToList(),
            new CampaignOverallProgressResponse(
                campaign.Participants.Count,
                completed,
                inProgress,
                notStarted,
                campaign.Participants.Sum(p => p.SubmittedSubmissions)));
    }

    public sealed record AudienceTargetDto(string Type, List<string> Values);

    public sealed record CreateCampaignRequest(
        string Name,
        string Description,
        DateTime StartDateUtc,
        DateTime EndDateUtc,
        int MinimumRequiredSubmissions,
        bool IsAnonymous,
        Guid CreatedByAdminId,
        List<AudienceTargetDto> AudienceTargets,
        List<Guid> ResolvedParticipantUserIds);

    public sealed record UpdateCampaignRequest(
        string Name,
        string Description,
        DateTime StartDateUtc,
        DateTime EndDateUtc,
        int MinimumRequiredSubmissions,
        bool IsAnonymous,
        List<AudienceTargetDto> AudienceTargets,
        List<Guid>? ResolvedParticipantUserIds);

    public sealed record SubmitCampaignFeedbackRequest(
        Guid RecipientUserId,
        Guid SubmittedByUserId,
        int Rating,
        string Comment,
        string Visibility,
        string[] Tags);

    public sealed record CampaignListItem(
        Guid Id,
        string Name,
        string Description,
        DateTime StartDateUtc,
        DateTime EndDateUtc,
        string Status,
        int MinimumRequiredSubmissions,
        bool IsAnonymous,
        DateTime CreatedAtUtc);

    public sealed record CampaignUserProgressResponse(
        Guid UserId,
        int RequiredSubmissions,
        int SubmittedSubmissions,
        string Status,
        DateTime? LastSubmissionAtUtc);

    public sealed record CampaignOverallProgressResponse(
        int TotalParticipants,
        int Completed,
        int InProgress,
        int NotStarted,
        int TotalSubmissions);

    public sealed record CampaignDetailsResponse(
        Guid Id,
        string Name,
        string Description,
        DateTime StartDateUtc,
        DateTime EndDateUtc,
        string Status,
        int MinimumRequiredSubmissions,
        bool IsAnonymous,
        Guid CreatedByAdminId,
        DateTime CreatedAtUtc,
        DateTime? UpdatedAtUtc,
        DateTime? ClosedAtUtc,
        List<AudienceTargetDto> AudienceTargets,
        List<Guid> ParticipantUserIds,
        CampaignOverallProgressResponse OverallProgress);
}

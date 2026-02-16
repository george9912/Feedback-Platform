namespace BFFService.API.Clients.Feddback
{
    public interface IFeedbackClient
    {
        Task<Guid> CreateFeedbackAsync(Guid userId, int rating, string comment, CancellationToken ct);
    }

}

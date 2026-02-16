namespace FeedbackService.API.Features.Clients
{
    public interface IUserClient
    {
        Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken);
    }
}

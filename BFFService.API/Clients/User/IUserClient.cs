namespace BFFService.API.Clients.User
{
    public interface IUserClient
    {
        Task<bool> UserExistsAsync(Guid userId, CancellationToken ct);
    }
}

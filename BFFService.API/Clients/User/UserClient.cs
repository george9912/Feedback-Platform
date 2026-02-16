namespace BFFService.API.Clients.User
{
    public class UserClient : IUserClient
    {
        private readonly HttpClient _http;

        public UserClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<bool> UserExistsAsync(Guid userId, CancellationToken ct)
        {
            var response = await _http.GetAsync($"/api/users/{userId}/exists", ct);
            return response.IsSuccessStatusCode;
        }
    }
}

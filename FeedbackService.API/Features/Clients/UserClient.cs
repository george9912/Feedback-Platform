namespace FeedbackService.API.Features.Clients
{
    //Mutata pe Infrastructure
    public class UserClient : IUserClient
    {
        private readonly HttpClient _http;

        public UserClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken)
        {
            var response = await _http.GetAsync($"/api/users/{userId}/exists", cancellationToken);
            return response.IsSuccessStatusCode;
        }
    }
}

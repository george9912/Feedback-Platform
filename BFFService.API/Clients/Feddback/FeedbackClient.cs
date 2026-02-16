namespace BFFService.API.Clients.Feddback
{
    public class FeedbackClient : IFeedbackClient
    {
        private readonly HttpClient _http;

        public FeedbackClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<Guid> CreateFeedbackAsync(Guid userId, int rating, string comment, CancellationToken ct)
        {
            var payload = new
            {
                UserId = userId,
                Rating = rating,
                Comment = comment
            };

            var response = await _http.PostAsJsonAsync("/api/feedback", payload, ct);
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadFromJsonAsync<CreateFeedbackResponse>(cancellationToken: ct);
            return data.Id;
        }

        private class CreateFeedbackResponse
        {
            public Guid Id { get; set; }
        }
    }
}

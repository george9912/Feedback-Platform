using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NotificationService.Clients
{
    public sealed class UserDto
    {
        public Guid Id { get; set; }
        public string? FirstName { get; set; }
        public string? Email { get; set; }
    }

    public interface IUserClient
    {
        Task<UserDto?> GetUserAsync(Guid userId, CancellationToken ct);
    }

    public sealed class UserClient : IUserClient
    {
        private readonly HttpClient _http;
        private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

        public UserClient(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("UserService");
        }

        public async Task<UserDto?> GetUserAsync(Guid userId, CancellationToken ct)
        {
            using var res = await _http.GetAsync($"/api/users/{userId}", ct);

            if (res.StatusCode == HttpStatusCode.NotFound)
                return null;

            res.EnsureSuccessStatusCode();

            var json = await res.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<UserDto>(json, JsonOpts);
        }
    }
}

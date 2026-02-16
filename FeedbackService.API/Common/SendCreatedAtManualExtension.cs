using FastEndpoints;

namespace FeedbackService.API.Common
{
    public static class SendCreatedAtManualExtension
    {
        public static async Task SendCreatedAtManual<TResponse>(
            this BaseEndpoint endpoint,
            string locationUrl,
            TResponse response,
            CancellationToken ct = default)
        {
            endpoint.HttpContext.Response.StatusCode = StatusCodes.Status201Created;
            endpoint.HttpContext.Response.Headers.Location = locationUrl;
            await endpoint.HttpContext.Response.WriteAsJsonAsync(response, cancellationToken: ct);
        }
    }
}

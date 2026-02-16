using BFFService.API.Clients.Feddback;
using BFFService.API.Clients.User;
using Microsoft.AspNetCore.Mvc;

namespace BFFService.API.Controllers
{
    [ApiController]
    [Route("api/bff/feedback")]
    public class CreateFeedbackController : ControllerBase
    {
        private readonly IUserClient _userClient;
        private readonly IFeedbackClient _feedbackClient;

        public CreateFeedbackController(IUserClient userClient, IFeedbackClient feedbackClient)
        {
            _userClient = userClient;
            _feedbackClient = feedbackClient;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateFeedbackRequest request, CancellationToken ct)
        {
            var userExists = await _userClient.UserExistsAsync(request.UserId, ct);
            if (!userExists)
            {
                return NotFound(new { message = "User not found." });
            }

            var feedbackId = await _feedbackClient.CreateFeedbackAsync(request.UserId, request.Rating, request.Comment, ct);
            return Created($"/api/feedback/{feedbackId}", new { feedbackId });
        }
    }

    public class CreateFeedbackRequest
    {
        public Guid UserId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }

}

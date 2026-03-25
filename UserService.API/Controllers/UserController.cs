using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserService.Application.DTOs;
using UserService.Application.Services;

namespace UserService.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IGraphUserSyncService _graphUserSyncService;

        public UserController(
            IUserService userService,
            IGraphUserSyncService graphUserSyncService)
        {
            _userService = userService;
            _graphUserSyncService = graphUserSyncService;
        }

        [HttpGet("me")]
        public IActionResult GetMe()
        {
            return Ok(new
            {
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                Name = User.Identity?.Name,
                Claims = User.Claims.Select(c => new
                {
                    c.Type,
                    c.Value
                })
            });
        }

        [HttpPost("sync-me")]
        public async Task<IActionResult> SyncMe()
        {
            var objectId =
                User.FindFirst("oid")?.Value ??
                User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

            var email =
                User.FindFirst("preferred_username")?.Value ??
                User.FindFirst(ClaimTypes.Email)?.Value ??
                User.FindFirst("emails")?.Value;

            var displayName =
                User.FindFirst("name")?.Value ??
                User.Identity?.Name;

            var firstName = User.FindFirst(ClaimTypes.GivenName)?.Value;
            var lastName = User.FindFirst(ClaimTypes.Surname)?.Value;

            if (string.IsNullOrWhiteSpace(objectId))
                return Unauthorized("Could not find Azure AD object id in token.");

            var result = await _userService.EnsureCurrentUserExistsAsync(new EnsureCurrentUserDto
            {
                AzureAdObjectId = objectId,
                Email = email,
                DisplayName = displayName,
                FirstName = firstName,
                LastName = lastName
            });

            return Ok(result);
        }

        [HttpGet("my-profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var objectId =
                User.FindFirst("oid")?.Value ??
                User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

            var email =
                User.FindFirst("preferred_username")?.Value ??
                User.FindFirst(ClaimTypes.Email)?.Value ??
                User.FindFirst("emails")?.Value;

            if (string.IsNullOrWhiteSpace(objectId) && string.IsNullOrWhiteSpace(email))
                return Unauthorized("Could not identify the current user from token claims.");

            var profile = await _userService.GetUserByAzureOidOrEmailAsync(objectId, email);

            if (profile == null)
                return NotFound("Profile for logged-in user was not found.");

            return Ok(profile);
        }

        [HttpPost("sync-tenant")]
        public async Task<IActionResult> SyncTenant(CancellationToken cancellationToken)
        {
            var tenantUsers = await _graphUserSyncService.GetTenantUsersAsync(cancellationToken);

            var syncedCount = 0;

            foreach (var graphUser in tenantUsers)
            {
                var email = graphUser.Mail ?? graphUser.UserPrincipalName ?? string.Empty;

                await _userService.UpsertFromGraphAsync(new UpsertGraphUserDto
                {
                    AzureAdObjectId = graphUser.AzureAdObjectId,
                    Email = email,
                    UserPrincipalName = graphUser.UserPrincipalName ?? string.Empty,
                    DisplayName = graphUser.DisplayName,
                    FirstName = graphUser.GivenName ?? string.Empty,
                    LastName = graphUser.Surname ?? string.Empty
                });

                syncedCount++;
            }

            return Ok(new
            {
                TotalFromGraph = tenantUsers.Count,
                SyncedToLocalDb = syncedCount
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto userDto)
        {
            var userId = await _userService.CreateUserAsync(userDto);

            return CreatedAtAction(
                nameof(GetUserById),
                new { id = userId },
                userId
            );
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var result = await _userService.GetUserByIdAsync(id);

            if (result == null || result.IsFailure)
                return NotFound($"User with ID {id} not found.");

            return Ok(result.Value);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] CreateUserDto userDto)
        {
            var success = await _userService.UpdateUserAsync(id, userDto);

            if (!success)
                return NotFound($"User with ID {id} not found.");

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var success = await _userService.DeleteUserAsync(id);

            if (!success)
                return NotFound($"User with ID {id} not found.");

            return NoContent();
        }

        [HttpGet("{id}/exists")]
        public async Task<IActionResult> CheckUserExists(Guid id)
        {
            var result = await _userService.GetUserByIdAsync(id);

            if (result == null || result.IsFailure)
                return NotFound(new { exists = false });

            return Ok(new { exists = true });
        }
    }
}
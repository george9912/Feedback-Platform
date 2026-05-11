using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserService.Application.DTOs;
using UserService.Application.Services;

namespace UserService.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    //[Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITenantUserSyncAppService _tenantUserSyncAppService;

        public UserController(
            IUserService userService,
            ITenantUserSyncAppService tenantUserSyncAppService)
        {
            _userService = userService;
            _tenantUserSyncAppService = tenantUserSyncAppService;
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

            var profile = await _userService.GetUserByAzureAdObjectIdOrEmailAsync(objectId, email);

            if (profile == null)
                return NotFound("Profile for logged-in user was not found.");

            return Ok(profile);
        }

        [HttpPost("sync-tenant")]
        public async Task<IActionResult> SyncTenantUsers(CancellationToken cancellationToken)
        {
            var result = await _tenantUserSyncAppService.SyncTenantUsersAsync(cancellationToken);
            return Ok(result);
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

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var result = await _userService.GetUserByIdAsync(id);

            if (result.IsFailure)
                return NotFound($"User with ID {id} not found.");

            return Ok(result.Value);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] CreateUserDto userDto)
        {
            var success = await _userService.UpdateUserAsync(id, userDto);

            if (!success)
                return NotFound($"User with ID {id} not found.");

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var success = await _userService.DeleteUserAsync(id);

            if (!success)
                return NotFound($"User with ID {id} not found.");

            return NoContent();
        }

        [HttpGet("{id:guid}/exists")]
        public async Task<IActionResult> CheckUserExists(Guid id)
        {
            var result = await _userService.GetUserByIdAsync(id);

            if (result.IsFailure)
                return NotFound(new { exists = false });

            return Ok(new { exists = true });
        }
    }
}
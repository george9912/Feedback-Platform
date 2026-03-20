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

        public UserController(IUserService userService)
        {
            _userService = userService;
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
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
                return NotFound($"User with ID {id} not found.");

            return Ok(user);
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

            if (result.IsFailure)
            {
                if (result.Error == "User not found")
                    return NotFound(new { exists = false });

                return StatusCode(500, result.Error);
            }

            return Ok(new { exists = true });
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

            // varianta recomandata: cautare dupa oid
            // fallback: dupa email
            var profile = await _userService.GetUserByAzureOidOrEmailAsync(objectId, email);

            if (profile == null)
                return NotFound("Profile for logged-in user was not found.");

            return Ok(profile);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs;
using UserService.Application.Services;
using static System.Net.WebRequestMethods;

namespace UserService.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    //De adaugat rute - substantive - plural
    //De facut research pentru controllere - sa foloseasca minimal API sau ceva clasa
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto userDto)
        {
            var userId = await _userService.CreateUserAsync(userDto);
            return CreatedAtAction(nameof(GetUserById), new { id = userId }, userId);
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

        //De vazut Result pattern pentru returnari mai complexe ( validari etc ) 
        //[HttpGet("{id}/exists")]
        //public async Task<IActionResult> CheckUserExists(Guid id)
        //{
        //    try
        //    {
        //        var user = await _userService.GetUserByIdAsync(id);
        //        if (user == null)
        //            return NotFound();

        //        return Ok(new { exists = true });
        //    }
        //    catch (Exception ex)
        //    {
        //        // log exception sau măcar temporar vezi în response
        //        return StatusCode(500, $"Internal error: {ex.Message}");
        //    }
        //}

        //With Result pattern
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

    }
}

using System.Security.Claims;
using InteractHub.Api.Data;
using InteractHub.Api.Dtos.Posts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using InteractHub.Api.Services;

namespace InteractHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMyProfile()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var user = await _userService.GetUserProfileAsync(currentUserId);

            if (user is null)
                return NotFound("User not found.");

            return Ok(user);
        }

        //[HttpGet("{id}/posts")]
        //public async Task<IActionResult> GetPostsByUserId([FromRoute] string id)
        //{
        //    var userExists = await _userService.GetPostsByUserIdAsync(id);
        //    if (userExists is null)
        //    {
        //        return NotFound($"User with id = {id} was not found.");
        //    }

        //    return Ok(userExists);
        //}

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserProfile([FromRoute] string id)
        {
            var user = await _userService.GetUserProfileAsync(id);

            if (user is null)
            {
                return NotFound($"User with id = {id} was not found.");
            }

            return Ok(user);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Ok(new List<object>()); // Trả về mảng rỗng nếu không có từ khoá
            }

            var users = await _userService.SearchUsersAsync(q);
            return Ok(users);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser([FromRoute] string id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result)
            {
                return NotFound($"User with id = {id} was not found.");
            }

            return Ok(new { message = "User deleted successfully." });
        }
    }
}

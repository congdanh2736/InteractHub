using InteractHub.Api.Dtos.Friends;
using InteractHub.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InteractHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FriendsController : ControllerBase
    {
        private readonly IFriendService _friendService;
        public FriendsController(IFriendService friendService)
        {
            _friendService = friendService;
        }

        [HttpPost("request")]
        public async Task<IActionResult> SendRequest([FromBody] SendFriendRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _friendService.SendFriendRequestAsync(userId, request.ReceiverId);
            if (result is null) return BadRequest("Invalid request or user not found.");

            return Ok(result);
        }

        [HttpPut("respond")]
        public async Task<IActionResult> RespondToRequest([FromBody] RespondFriendRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _friendService.RespondToRequestAsync(userId, request.RequesterId, request.Status);
            if (result is null) return BadRequest("Request not found or invalid status.");

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyFriend()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var friends = await _friendService.GetMyFriendAsync(userId);
            return Ok(friends);
        }

        [HttpGet("request")]
        public async Task<IActionResult> GetPendingRequest()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var requests = await _friendService.GetPendingRequestAsync(userId);
            return Ok(requests);
        }

        [HttpGet("status/{targetUserId}")]
        public async Task<IActionResult> GetFriendshipStatus([FromRoute] string targetUserId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _friendService.GetFriendshipStatusAsync(userId, targetUserId);
            return Ok(result);
        }

        [HttpDelete("{targetUserId}")]
        public async Task<IActionResult> Unfriend([FromRoute] string targetUserId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var success = await _friendService.UnfriendAsync(userId, targetUserId);
            if (!success) return BadRequest("Friendship not found.");

            return Ok(new { message = "Unfriended or cancelled request successfully." });
        }
    }
}

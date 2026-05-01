using InteractHub.Api.Data;
using InteractHub.Api.Dtos.Likes;
using InteractHub.Api.Entities;
using InteractHub.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InteractHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LikesController : ControllerBase
    {
        private readonly ILikeService _likeService;

        public LikesController(ILikeService likeService)
        {
            _likeService = likeService;
        }

        [HttpPost("toggle")]
        [Authorize]
        public async Task<IActionResult> ToggleLike([FromBody] ToggleLikeRequest request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("Can't find user infomation.");
            }

            var postExists = await _likeService.ToggleLikeAsync(request, currentUserId);
            if (postExists is null) 
                return NotFound("Post not found");

            return Ok(postExists);
        }
    }
}

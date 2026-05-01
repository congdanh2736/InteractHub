using InteractHub.Api.Dtos.Stories;
using InteractHub.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InteractHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StoriesController : ControllerBase
    {
        private readonly IStoryService _storyService;
        public StoriesController(IStoryService storyService)
        {
            _storyService = storyService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateStory([FromBody] CreateStoryRequest request)
        {
            if (string.IsNullOrEmpty(request.ImageUrl))
                return BadRequest("ImageUrl is required.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _storyService.CreateStoryAsync(request, userId);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetActiveStories()
        {
            var stories = await _storyService.GetActiveStoriesAsync();
            return Ok(stories);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteStory([FromRoute] int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _storyService.DeleteStoryAsync(id, userId);
            if (!success)
                return StatusCode(StatusCodes.Status401Unauthorized, "Story not found or unauthorized.");

            return Ok(new
            {
                message = "Story deleted successfully.",
                storyId = id
            });
        }
    }
}

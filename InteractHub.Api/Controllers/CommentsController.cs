using InteractHub.Api.Data;
using InteractHub.Api.Dtos.Comments;
using InteractHub.Api.Entities;
using InteractHub.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using InteractHub.Api.Hubs;

namespace InteractHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequest request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("Can't find user infomation.");
            }

            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest("Content is required.");
            }

            var result = await _commentService.CreateCommentAsync(request, currentUserId);
            if (result is null)
                return NotFound("Post not found");

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment([FromRoute] int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var success = await _commentService.DeleteCommentAsync(id, userId);
            if (!success) 
                return StatusCode(StatusCodes.Status403Forbidden, "Comment not found or unauthorized.");

            return Ok(new 
            { 
                message = "Comment deleted successfully", 
                commentId = id 
            });
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> UpdateComment([FromRoute] int id, [FromBody] UpdateCommentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest("Content is required.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _commentService.UpdateCommentAsync(id, request, userId);
            if (result == null) 
                return StatusCode(StatusCodes.Status403Forbidden, "Comment not found or unauthorized.");

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetCommentByIdAsync([FromRoute] int id)
        {
            var result = await _commentService.GetCommentAsync(id);

            if (result is null)
                return StatusCode(StatusCodes.Status404NotFound, "Comment not found");

            return Ok(result);
        }
    }
}

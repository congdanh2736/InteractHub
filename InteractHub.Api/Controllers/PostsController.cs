using InteractHub.Api.Services;
using InteractHub.Api.Dtos.Posts;
using InteractHub.Api.Dtos.Comments;
using InteractHub.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace InteractHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostsController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest("Content is required.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized("Can't find user info in token.");
            }

            var result = await _postService.CreatePostAsync(request, userId);

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetPosts([FromQuery] string? keyword, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 0) pageNumber = 1;
            if (pageSize < 0) pageSize = 10;
            if (pageSize > 50) pageSize = 50;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _postService.GetAllPostAsync(keyword, pageNumber, pageSize, userId);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPostById([FromRoute] int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var post = await _postService.GetPostByIdAsync(id, userId);

            if (post is null)
            {
                return NotFound($"Post with id = {id} was not found.");
            }

            return Ok(post);
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> DeletePost([FromRoute] int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) 
                return Unauthorized();

            var isDeleted = await _postService.DeletePostAsync(id, userId);
            if (!isDeleted) 
                return StatusCode(StatusCodes.Status403Forbidden, "Post not found or you are not the author.");

            return Ok(new { 
                message = "Post deleted successfully.", 
                postId = id 
            });
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> UpdatePost([FromRoute] int id, [FromBody] UpdatePostRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Content)) return BadRequest("Content is required.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var result = await _postService.UpdatePostAsync(id, request, userId);
            if (result is null) return StatusCode(StatusCodes.Status403Forbidden, "Post not found or you are not the author.");

            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetPostsByUser([FromRoute] string userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _postService.GetPostsByUserIdAsync(userId, currentUserId, pageNumber, pageSize);
            return Ok(result);
        }

        //[HttpGet("{id:int}/comments")]
        //public async Task<IActionResult> GetCommentsByPostId([FromRoute] int id)
        //{
        //    var postExists = await _context.Posts.AnyAsync(p => p.Id == id);
        //    if (!postExists)
        //    {
        //        return NotFound($"Post with id = {id} was not found.");
        //    }

        //    var comment = await _context.Comments
        //        .AsNoTracking()
        //        .Where(c => c.PostId == id)
        //        .Include(c => c.User)
        //        .OrderByDescending(c => c.CreatedAt)
        //        .Select(c => new CommentResponse
        //        {
        //            Id = c.Id,
        //            PostId = c.PostId,
        //            UserId = c.UserId,
        //            UserDisplayName = c.User.DisplayName,
        //            Content = c.Content,
        //            CreatedAt = c.CreatedAt,
        //        })
        //        .ToListAsync();

        //    return Ok(comment);
        //}

        
    }
}

using InteractHub.Api.Dtos.Hashtags;
using InteractHub.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace InteractHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HashtagsController : ControllerBase
    {
        private readonly IHashtagService _hashtagService;
        public HashtagsController(IHashtagService hashtagService)
        {
            _hashtagService = hashtagService;
        }

        [HttpGet("trending")]
        public async Task<IActionResult> GetTrending()
        {
            var trending = await _hashtagService.GetTrendingAsync();
            return Ok(trending);
        }

        [HttpPost]
        public async Task<IActionResult> CreateHashtag([FromBody] HashtagDtos request)
        {
            var tag = await _hashtagService.CreateHashtagAsync(request);

            if (tag is null)
            {
                return BadRequest("Tag is exist or name is empty.");
            }

            return Ok(tag);
        }
    }
}

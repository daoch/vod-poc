using backend.Interfaces;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    [Route("api/bunny/videos")]
    [ApiController]
    public class BunnyVideosController : ControllerBase
    {
        private readonly IVideoService _service;

        public BunnyVideosController(IVideoService service)
        {
            _service = service;
        }

        //[Authorize]
        [HttpGet("list/{libraryId}")]
        public async Task<IActionResult> ListVideos(
            [FromRoute] int libraryId,
            [FromQuery] int page = 1,
            [FromQuery] int itemsPerPage = 100,
            [FromQuery] string? search = null,
            CancellationToken ct = default)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var data = await _service.ListVideosAsync(libraryId, userId, page, itemsPerPage, search, ct);
            if (data is null) return StatusCode(502);

            return Ok(data);
        }

        [Authorize]
        [HttpPost("upload/{libraryId}")]
        public async Task<IActionResult> Upload(int libraryId, IFormFile file, CancellationToken ct)
        {
            using var stream = file.OpenReadStream();

            var videoId = await _service.UploadVideoAsync(
                libraryId,
                stream,
                file.FileName,
                ct
            );

            return Ok(new { videoId });
        }

        private string? GetUserId()
        => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}

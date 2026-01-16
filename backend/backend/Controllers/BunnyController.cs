using backend.Interfaces;
using backend.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/bunny")]
    public sealed class BunnyController : ControllerBase
    {
        private readonly IBunnyEmbedSigner _signer;
        private readonly IVideoProgressStore _progress;

        public BunnyController(IBunnyEmbedSigner signer, IVideoProgressStore progress)
        {
            _signer = signer;
            _progress = progress;
        }

        [Authorize]
        [HttpGet("embed-url/{videoId}")]
        public ActionResult<BunnyEmbedResponse> GetEmbedUrl(string videoId, int ttlSeconds = 900)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            videoId = videoId.Trim();

            var startAtSeconds = GetStartAtSeconds(userId, videoId);

            var url = _signer.BuildSignedEmbedUrl(videoId, TimeSpan.FromSeconds(ttlSeconds));

            return Ok(new BunnyEmbedResponse
            {
                EmbedUrl = url,
                StartAtSeconds = startAtSeconds
            });
        }

        [Authorize]
        [HttpGet("stream-url/{videoId}")]
        public ActionResult<BunnyStreamResponse> GetStreamUrl(string videoId, int ttlSeconds = 900)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            videoId = videoId.Trim();

            var startAtSeconds = GetStartAtSeconds(userId, videoId);

            var url = _signer.BuildSignedStreamUrl(videoId, TimeSpan.FromSeconds(ttlSeconds));

            return Ok(new BunnyStreamResponse
            {
                StreamUrl = url,
                StartAtSeconds = startAtSeconds
            });
        }

        private string? GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        private double GetStartAtSeconds(string userId, string videoId)
        {
            if (_progress.TryGet(userId, videoId, out var rec) && rec is not null)
            {
                var safe = rec.PositionSeconds;

                if (rec.DurationSeconds > 0)
                    safe = Math.Min(safe, Math.Max(0, rec.DurationSeconds - 2));

                return Math.Max(0, safe);
            }

            return 0;
        }
    }
}

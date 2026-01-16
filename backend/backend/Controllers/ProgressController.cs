using backend.Interfaces;
using backend.Models;
using backend.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/progress")]
    public sealed class ProgressController : ControllerBase
    {
        private readonly IVideoProgressStore _store;

        public ProgressController(IVideoProgressStore store)
        {
            _store = store;
        }

        private string? GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        [HttpPost]
        [Authorize]
        public ActionResult<ProgressResponse> Upsert([FromBody] UpsertProgressRequest req)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(req.VideoId))
                return BadRequest("VideoId is required.");

            var videoId = req.VideoId.Trim();
            var position = Math.Max(0, req.PositionSeconds);
            var duration = req.DurationSeconds;

            // regla de completado (90%)
            var completed = duration > 0 && position >= duration * 0.90;

            var record = new VideoProgressRecord
            {
                UserId = userId,
                VideoId = videoId,
                PositionSeconds = position,
                DurationSeconds = duration,
                Completed = completed,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _store.Upsert(record);

            return Ok(new ProgressResponse
            {
                VideoId = record.VideoId,
                PositionSeconds = record.PositionSeconds,
                DurationSeconds = record.DurationSeconds,
                Completed = record.Completed,
                UpdatedAt = record.UpdatedAt
            });
        }

        [HttpGet("{videoId}")]
        [Authorize]
        public ActionResult<ProgressResponse> Get(string videoId)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            if (_store.TryGet(userId, videoId.Trim(), out var record) && record is not null)
            {
                return Ok(new ProgressResponse
                {
                    VideoId = record.VideoId,
                    PositionSeconds = record.PositionSeconds,
                    DurationSeconds = record.DurationSeconds,
                    Completed = record.Completed,
                    UpdatedAt = record.UpdatedAt
                });
            }

            return NotFound();
        }

        [HttpGet]
        [Authorize]
        public ActionResult<IReadOnlyCollection<ProgressResponse>> List()
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var rows = _store.ListByUser(userId)
                .Select(r => new ProgressResponse
                {
                    VideoId = r.VideoId,
                    PositionSeconds = r.PositionSeconds,
                    DurationSeconds = r.DurationSeconds,
                    Completed = r.Completed,
                    UpdatedAt = r.UpdatedAt
                })
                .ToArray();

            return Ok(rows);
        }
    }
}

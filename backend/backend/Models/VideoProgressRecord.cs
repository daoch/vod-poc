namespace backend.Models
{
    public sealed class VideoProgressRecord
    {
        public required string UserId { get; init; }
        public required string VideoId { get; init; }

        public double PositionSeconds { get; set; }
        public double DurationSeconds { get; set; }

        public bool Completed { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}

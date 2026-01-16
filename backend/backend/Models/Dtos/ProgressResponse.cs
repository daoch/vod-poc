namespace backend.Models.Dtos
{
    public sealed class ProgressResponse
    {
        public required string VideoId { get; init; }
        public double PositionSeconds { get; init; }
        public double DurationSeconds { get; init; }
        public bool Completed { get; init; }
        public DateTimeOffset UpdatedAt { get; init; }
    }
}

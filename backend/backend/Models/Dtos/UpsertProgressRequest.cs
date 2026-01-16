namespace backend.Models.Dtos
{
    public sealed class UpsertProgressRequest
    {
        public required string VideoId { get; init; }
        public double PositionSeconds { get; init; }
        public double DurationSeconds { get; init; }
    }
}

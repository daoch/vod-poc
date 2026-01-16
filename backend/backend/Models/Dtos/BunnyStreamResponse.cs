namespace backend.Models.Dtos
{
    public sealed class BunnyStreamResponse
    {
        public required string StreamUrl { get; init; }
        public double StartAtSeconds { get; init; }
    }
}

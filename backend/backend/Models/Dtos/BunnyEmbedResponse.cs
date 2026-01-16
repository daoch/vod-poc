namespace backend.Models.Dtos
{
    public sealed class BunnyEmbedResponse
    {
        public required string EmbedUrl { get; init; }
        public double StartAtSeconds { get; init; }
    }
}

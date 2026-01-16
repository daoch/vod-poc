namespace backend.Models.Options
{
    public sealed class BunnyOptions
    {
        public int VideoLibraryId { get; set; }
        public string TokenSecurityKey { get; set; } = "";
        public string EmbedBaseUrl { get; set; } = "https://iframe.mediadelivery.net/embed";
        public string CdnHostname { get; set; } = "";
    }
}

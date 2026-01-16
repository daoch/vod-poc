using System.Text.Json;

namespace backend.Models.Dtos
{
    public class VideoListResponse
    {
        public int CurrentPage { get; set; }
        public int ItemsPerPage { get; set; }
        public int TotalItems { get; set; }
        public bool HasMoreItems { get; set; }
        public List<BunnyVideoItem> Items { get; set; } = new();
    }

    public class BunnyVideoItem
    {
        public string Guid { get; set; } = "";
        public string Title { get; set; } = "";
        public JsonElement Status { get; set; }
        public DateTime DateUploaded { get; set; }

        public string AvailableResolutions { get; set; } = "";
        public double Length { get; set; }
        public long StorageSize { get; set; }
        public string? ThumbnailFileName { get; set; }
        public string ThumbnailLink { get; set; } = "";
        public double? ProgressPercent { get; set; }
    }
}

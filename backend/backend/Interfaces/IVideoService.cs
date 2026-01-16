using backend.Models.Dtos;

namespace backend.Interfaces
{
    public interface IVideoService
    {
        Task<VideoListResponse?> ListVideosAsync(
            int libraryId,
            string userId,
            int page = 1,
            int itemsPerPage = 100,
            string? search = null,
            CancellationToken ct = default
        );


        Task<string> UploadVideoAsync(
            int libraryId,
            Stream fileStream,
            string fileName,
            CancellationToken ct = default
        );
    }
}

using backend.Models;

namespace backend.Interfaces
{
    public interface IVideoProgressStore
    {
        VideoProgressRecord Upsert(VideoProgressRecord record);
        bool TryGet(string userId, string videoId, out VideoProgressRecord? record);
        IReadOnlyCollection<VideoProgressRecord> ListByUser(string userId);
    }
}

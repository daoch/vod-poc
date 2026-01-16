using backend.Interfaces;
using backend.Models;
using System.Collections.Concurrent;

namespace backend.Services
{
    public sealed class InMemoryVideoProgressStore : IVideoProgressStore
    {
        private readonly ConcurrentDictionary<(string UserId, string VideoId), VideoProgressRecord> _table = new();

        public VideoProgressRecord Upsert(VideoProgressRecord record)
        {
            var key = (record.UserId, record.VideoId);
            _table.AddOrUpdate(key, record, (_, __) => record);
            return record;
        }

        public bool TryGet(string userId, string videoId, out VideoProgressRecord? record)
        {
            var ok = _table.TryGetValue((userId, videoId), out var value);
            record = value;
            return ok;
        }

        public IReadOnlyCollection<VideoProgressRecord> ListByUser(string userId)
        {
            return _table
                .Where(kv => kv.Key.UserId == userId)
                .Select(kv => kv.Value)
                .OrderByDescending(x => x.UpdatedAt)
                .ToArray();
        }
    }
}

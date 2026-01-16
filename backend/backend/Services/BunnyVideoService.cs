using backend.Interfaces;
using backend.Models.Dtos;

namespace backend.Services
{
    public class BunnyVideoService : IVideoService
    {
        private readonly HttpClient _http;
        private readonly string _streamApiKey;
        private readonly IVideoProgressStore _progress;

        public BunnyVideoService(HttpClient http, IConfiguration cfg, IVideoProgressStore progress)
        {
            _http = http;
            _progress = progress;

            _streamApiKey = cfg["Bunny:StreamApiKey"]
                ?? throw new Exception("Bunny Stream API Key no configurada");
        }

        public async Task<VideoListResponse?> ListVideosAsync(int libraryId, string userId, int page = 1, int itemsPerPage = 100, string? search = null, CancellationToken ct = default)
        {
            var url = $"https://video.bunnycdn.com/library/{libraryId}/videos" +
                      $"?page={page}&itemsPerPage={itemsPerPage}";

            if (!string.IsNullOrWhiteSpace(search))
                url += $"&search={Uri.EscapeDataString(search)}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("AccessKey", _streamApiKey);

            var response = await _http.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode) return null;

            var data = await response.Content.ReadFromJsonAsync<VideoListResponse>(cancellationToken: ct);
            if (data is null) return null;

            if (data.Items is not null)
            {
                foreach (var v in data.Items)
                {
                    v.ThumbnailLink = $"https://vz-dc89dd2e-a9d.b-cdn.net/{v.Guid}/{v.ThumbnailFileName}".Trim();

                    if (_progress.TryGet(userId, v.Guid, out var rec) && rec is not null)
                    {
                        // prioridad: duration guardada en progreso
                        var duration = rec.DurationSeconds;

                        // fallback: length que viene del listado de Bunny (si duration es 0)
                        if (duration <= 0 && v.Length > 0)
                            duration = v.Length;

                        if (duration > 0)
                        {
                            var percent = (rec.PositionSeconds / duration) * 100.0;

                            // clamp 0..100
                            if (percent < 0) percent = 0;
                            if (percent > 100) percent = 100;

                            // opcional: redondear bonito
                            v.ProgressPercent = Math.Round(percent, 1);
                        }
                        else
                        {
                            // si no podemos calcular, igual lo mandamos null
                            v.ProgressPercent = null;
                        }
                    }
                    else
                    {
                        v.ProgressPercent = null;
                    }
                }
            }

            return data;
        }

        public async Task<string> UploadVideoAsync(int libraryId, Stream fileStream, string fileName, CancellationToken ct = default)
        {
            // Create the video
            
            var createUrl = $"https://video.bunnycdn.com/library/{libraryId}/videos";

            var createRequest = new HttpRequestMessage(HttpMethod.Post, createUrl)
            {
                Content = JsonContent.Create(new { title = fileName })
            };

            createRequest.Headers.Add("AccessKey", _streamApiKey);

            var createResponse = await _http.SendAsync(createRequest, ct);
            if (!createResponse.IsSuccessStatusCode)
                throw new Exception($"Error creating Bunny video: {createResponse.StatusCode}");

            var createJson = await createResponse.Content.ReadFromJsonAsync<CreateVideoResponse>(cancellationToken: ct);
            if (createJson == null || string.IsNullOrWhiteSpace(createJson.Guid))
                throw new Exception("Bunny did not return a valid video GUID.");

            var videoId = createJson.Guid;

            // Upload video

            var uploadUrl = $"https://video.bunnycdn.com/library/{libraryId}/videos/{videoId}";

            var uploadRequest = new HttpRequestMessage(HttpMethod.Put, uploadUrl)
            {
                Content = new StreamContent(fileStream)
            };
            uploadRequest.Headers.Add("AccessKey", _streamApiKey);
            uploadRequest.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4");

            var uploadResponse = await _http.SendAsync(uploadRequest, ct);
            if (!uploadResponse.IsSuccessStatusCode)
                throw new Exception($"Error uploading video data to Bunny: {uploadResponse.StatusCode}");

            return videoId;
        }
    }
}

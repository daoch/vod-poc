using backend.Interfaces;
using backend.Models.Options;
using System.Security.Cryptography;
using System.Text;

namespace backend.Services
{
    public sealed class BunnyEmbedSigner : IBunnyEmbedSigner
    {
        private readonly BunnyOptions _opt;

        public BunnyEmbedSigner(BunnyOptions opt) => _opt = opt;

        public string BuildSignedEmbedUrl(string videoId, TimeSpan validFor, IDictionary<string, string?>? playerParams = null)
        {
            if (string.IsNullOrWhiteSpace(videoId)) throw new ArgumentException("videoId requerido");
            if (string.IsNullOrWhiteSpace(_opt.TokenSecurityKey)) throw new InvalidOperationException("TokenSecurityKey no configurado");

            var expiresUnix = DateTimeOffset.UtcNow.Add(validFor).ToUnixTimeSeconds();
            var token = Sha256Hex(_opt.TokenSecurityKey + videoId + expiresUnix);

            // URL base
            var sb = new StringBuilder();
            sb.Append(_opt.EmbedBaseUrl.TrimEnd('/'))
              .Append('/')
              .Append(_opt.VideoLibraryId)
              .Append('/')
              .Append(videoId);

            // Query: token + expires + params opcionales del player
            var query = new List<string>
            {
                $"token={Uri.EscapeDataString(token)}",
                $"expires={expiresUnix}"
            };

            if (playerParams != null)
            {
                foreach (var kv in playerParams)
                {
                    if (string.IsNullOrWhiteSpace(kv.Key) || kv.Value is null) continue;
                    query.Add($"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}");
                }
            }

            sb.Append('?').Append(string.Join("&", query));
            return sb.ToString();
        }

        public string BuildSignedStreamUrl(string videoId, TimeSpan validFor)
        {
            if (string.IsNullOrWhiteSpace(videoId))
                throw new ArgumentException("videoId requerido");

            if (string.IsNullOrWhiteSpace(_opt.CdnHostname))
                throw new InvalidOperationException("CDN Hostname no configurado");

            var expiresUnix = DateTimeOffset.UtcNow.Add(validFor).ToUnixTimeSeconds();

            var path = $"/{videoId}/playlist.m3u8";

            var token = Sha256Hex(_opt.TokenSecurityKey + path + expiresUnix);

            return $"https://{_opt.CdnHostname}{path}?token={token}&expires={expiresUnix}";
        }


        private static string Sha256Hex(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            var hex = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) hex.Append(b.ToString("x2"));
            return hex.ToString();
        }
    }
}

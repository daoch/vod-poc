namespace backend.Interfaces
{
    public interface IBunnyEmbedSigner
    {
        string BuildSignedEmbedUrl(string videoId, TimeSpan validFor, IDictionary<string, string?>? playerParams = null);
        string BuildSignedStreamUrl(string videoId, TimeSpan validFor);
    }
}

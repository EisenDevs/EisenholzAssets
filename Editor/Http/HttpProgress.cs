namespace Eisenholz.AssetCatalog.Editor.Http
{
    /// <summary>Progress snapshot for an in-flight download.</summary>
    public readonly struct HttpProgress
    {
        /// <summary>Bytes downloaded so far.</summary>
        public readonly ulong DownloadedBytes;

        /// <summary>Completion fraction in [0,1]; may be 0 until the server reports Content-Length.</summary>
        public readonly float Fraction;

        public HttpProgress(ulong downloadedBytes, float fraction)
        {
            DownloadedBytes = downloadedBytes;
            Fraction = fraction;
        }

        public static HttpProgress Complete(ulong downloadedBytes) => new HttpProgress(downloadedBytes, 1f);
    }
}

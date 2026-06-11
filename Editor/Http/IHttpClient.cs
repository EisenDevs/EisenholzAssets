using System;
using System.Threading;
using System.Threading.Tasks;

namespace Eisenholz.AssetCatalog.Editor.Http
{
    /// <summary>
    /// Minimal async HTTP surface used by the catalog. Abstracted so it can be faked in tests and
    /// swapped without touching call sites.
    /// </summary>
    public interface IHttpClient
    {
        /// <summary>GET the response body as text (used for JSON endpoints).</summary>
        Task<HttpResult<string>> GetTextAsync(string url, CancellationToken ct);

        /// <summary>GET the response body as raw bytes (used for thumbnails).</summary>
        Task<HttpResult<byte[]>> GetBytesAsync(string url, CancellationToken ct);

        /// <summary>
        /// GET a (potentially large) resource, streaming it straight to <paramref name="destPath"/>
        /// on disk rather than buffering in memory. Reports progress and supports cancellation.
        /// </summary>
        Task<HttpResult<bool>> DownloadToFileAsync(
            string url, string destPath, IProgress<HttpProgress> progress, CancellationToken ct);
    }
}

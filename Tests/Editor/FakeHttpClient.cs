using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Eisenholz.AssetCatalog.Editor.Http;

namespace Eisenholz.AssetCatalog.Tests
{
    /// <summary>In-memory <see cref="IHttpClient"/> for tests. Maps URLs to canned text responses.</summary>
    public sealed class FakeHttpClient : IHttpClient
    {
        readonly Dictionary<string, HttpResult<string>> m_TextResponses = new Dictionary<string, HttpResult<string>>();

        public string LastUrl { get; private set; }

        public void SetText(string url, string body, int status = 200) =>
            m_TextResponses[url] = HttpResult<string>.Ok(status, body);

        public void SetFailure(string url, HttpResult<string> failure) =>
            m_TextResponses[url] = failure;

        public Task<HttpResult<string>> GetTextAsync(string url, CancellationToken ct)
        {
            LastUrl = url;
            if (m_TextResponses.TryGetValue(url, out var result))
                return Task.FromResult(result);
            return Task.FromResult(HttpResult<string>.HttpError(404, "not found", null));
        }

        public Task<HttpResult<byte[]>> GetBytesAsync(string url, CancellationToken ct) =>
            Task.FromResult(HttpResult<byte[]>.Ok(200, Array.Empty<byte>()));

        public Task<HttpResult<bool>> DownloadToFileAsync(
            string url, string destPath, IProgress<HttpProgress> progress, CancellationToken ct) =>
            Task.FromResult(HttpResult<bool>.Ok(200, true));
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.Networking;

namespace Eisenholz.AssetCatalog.Editor.Http
{
    /// <summary>
    /// <see cref="IHttpClient"/> implementation over <see cref="UnityWebRequest"/>. All requests run
    /// on the main thread. Connection errors and 5xx responses are retried with exponential backoff;
    /// 4xx responses are returned immediately (retrying them is pointless).
    /// </summary>
    public sealed class CatalogHttpClient : IHttpClient
    {
        const int k_DefaultMaxRetries = 2;
        const int k_BaseBackoffMs = 250;

        readonly IAuthProvider m_Auth;
        readonly int m_TimeoutSeconds;
        readonly int m_MaxRetries;

        public CatalogHttpClient(IAuthProvider auth, int timeoutSeconds, int maxRetries = k_DefaultMaxRetries)
        {
            m_Auth = auth ?? new NullAuthProvider();
            m_TimeoutSeconds = timeoutSeconds;
            m_MaxRetries = Math.Max(0, maxRetries);
        }

        public Task<HttpResult<string>> GetTextAsync(string url, CancellationToken ct) =>
            SendWithRetryAsync(() => UnityWebRequest.Get(url), r => r.downloadHandler.text, null, ct);

        public Task<HttpResult<byte[]>> GetBytesAsync(string url, CancellationToken ct) =>
            SendWithRetryAsync(() => UnityWebRequest.Get(url), r => r.downloadHandler.data, null, ct);

        public Task<HttpResult<bool>> DownloadToFileAsync(
            string url, string destPath, IProgress<HttpProgress> progress, CancellationToken ct)
        {
            return SendWithRetryAsync(
                () =>
                {
                    var request = UnityWebRequest.Get(url);
                    request.downloadHandler = new DownloadHandlerFile(destPath) { removeFileOnAbort = true };
                    return request;
                },
                _ => true,
                progress,
                ct);
        }

        async Task<HttpResult<T>> SendWithRetryAsync<T>(
            Func<UnityWebRequest> factory, Func<UnityWebRequest, T> onSuccess,
            IProgress<HttpProgress> progress, CancellationToken ct)
        {
            HttpResult<T> result = HttpResult<T>.ConnectionError("Request was not attempted.");

            for (var attempt = 0; attempt <= m_MaxRetries; attempt++)
            {
                if (ct.IsCancellationRequested)
                    return HttpResult<T>.Canceled();

                using (var request = factory())
                    result = await SendOnceAsync(request, onSuccess, progress, ct);

                if (result.Outcome == HttpOutcome.Canceled || result.Outcome == HttpOutcome.Success)
                    return result;

                var retriable = result.Outcome == HttpOutcome.ConnectionError ||
                                (result.Outcome == HttpOutcome.HttpError && result.StatusCode >= 500);
                if (!retriable || attempt == m_MaxRetries)
                    return result;

                try
                {
                    await Task.Delay(k_BaseBackoffMs * (1 << attempt), ct);
                }
                catch (OperationCanceledException)
                {
                    return HttpResult<T>.Canceled();
                }
            }

            return result;
        }

        async Task<HttpResult<T>> SendOnceAsync<T>(
            UnityWebRequest request, Func<UnityWebRequest, T> onSuccess,
            IProgress<HttpProgress> progress, CancellationToken ct)
        {
            ApplyHeaders(request);
            if (m_TimeoutSeconds > 0)
                request.timeout = m_TimeoutSeconds;

            EditorApplication.CallbackFunction pump = null;
            if (progress != null)
            {
                pump = () => progress.Report(new HttpProgress(request.downloadedBytes, request.downloadProgress));
                EditorApplication.update += pump;
            }

            using (ct.Register(() => request.Abort()))
            {
                try
                {
                    await request.SendWebRequest();
                }
                catch (Exception ex)
                {
                    // SendWebRequest can throw synchronously (e.g. Unity blocking insecure HTTP, or a
                    // malformed URL) instead of surfacing via request.result. Convert to a result so the
                    // editor flow never sees an unhandled exception.
                    return HttpResult<T>.ConnectionError(DescribeSendException(ex, request.url));
                }
                finally
                {
                    if (pump != null)
                        EditorApplication.update -= pump;
                }
            }

            if (ct.IsCancellationRequested)
                return HttpResult<T>.Canceled();

            switch (request.result)
            {
                case UnityWebRequest.Result.Success:
                    progress?.Report(HttpProgress.Complete(request.downloadedBytes));
                    return HttpResult<T>.Ok((int)request.responseCode, onSuccess(request));

                case UnityWebRequest.Result.ProtocolError:
                    return HttpResult<T>.HttpError(
                        (int)request.responseCode, request.error, request.downloadHandler?.text);

                default:
                    return HttpResult<T>.ConnectionError(request.error);
            }
        }

        static string DescribeSendException(Exception ex, string url)
        {
            if (ex is InvalidOperationException && ex.Message.IndexOf("insecure", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Unity blocked an insecure HTTP request. Either set Project Settings ▸ Player ▸ " +
                       "Other Settings ▸ \"Allow downloads over HTTP\" to \"Always allowed\", or use an " +
                       "https:// catalog URL in the Asset Catalog settings.";

            return string.IsNullOrEmpty(ex.Message) ? ex.GetType().Name : ex.Message;
        }

        void ApplyHeaders(UnityWebRequest request)
        {
            request.SetRequestHeader("Accept", "application/json");
            if (m_Auth.TryGetAuthHeader(out var name, out var value))
                request.SetRequestHeader(name, value);
        }
    }
}

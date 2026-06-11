using Eisenholz.AssetCatalog.Editor.Http;

namespace Eisenholz.AssetCatalog.Editor.Utils
{
    /// <summary>
    /// Maps transport / HTTP failures onto short, user-facing messages so every surface
    /// (browser, details panel, settings, accelerator) reports errors the same friendly way
    /// instead of leaking raw enum + status-code + UnityWebRequest.error strings.
    /// </summary>
    public static class ErrorText
    {
        public static string Describe<T>(HttpResult<T> result)
        {
            if (result == null)
                return "Unknown error.";

            switch (result.Outcome)
            {
                case HttpOutcome.Success:
                    return string.Empty;
                case HttpOutcome.Canceled:
                    return "Canceled.";
                case HttpOutcome.ConnectionError:
                    // The client supplies an actionable message for known cases (e.g. insecure HTTP);
                    // fall back to generic guidance only when it didn't.
                    return string.IsNullOrEmpty(result.Error)
                        ? "Can't reach the catalog server. Check the URL in Settings and your network connection."
                        : result.Error;
                case HttpOutcome.HttpError:
                    return DescribeStatus(result.StatusCode, result.Error);
                default:
                    return string.IsNullOrEmpty(result.Error) ? "Unknown error." : result.Error;
            }
        }

        static string DescribeStatus(int status, string fallback)
        {
            switch (status)
            {
                case 400: return "The server rejected the request (bad request).";
                case 401:
                case 403: return "Access denied by the server (authorization required).";
                case 404: return "Not found on the server.";
                case 408: return "The server timed out. Try again.";
                case 429: return "Too many requests — slow down and try again.";
                case 500:
                case 502:
                case 503:
                case 504: return $"Server error ({status}). Try again shortly.";
                default:
                    return string.IsNullOrEmpty(fallback)
                        ? $"Request failed ({status})."
                        : $"Request failed ({status}): {fallback}";
            }
        }
    }
}

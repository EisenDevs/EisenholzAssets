namespace Eisenholz.AssetCatalog.Editor.Http
{
    /// <summary>Transport-level outcome of an HTTP call.</summary>
    public enum HttpOutcome
    {
        Success,
        HttpError,       // a response arrived with a 4xx/5xx status
        ConnectionError, // could not reach the server / timed out / DNS, etc.
        Canceled
    }

    /// <summary>
    /// Result of an HTTP call. Never thrown — transport and HTTP errors are returned as data so the
    /// editor flow (and AssetDatabase state) is never left half-finished by an exception.
    /// </summary>
    public sealed class HttpResult<T>
    {
        public HttpOutcome Outcome { get; }
        public int StatusCode { get; }
        public T Value { get; }

        /// <summary>Human-readable error (UnityWebRequest.error or a parse message). Null on success.</summary>
        public string Error { get; }

        /// <summary>Raw response body when an error carried one (useful for parsing the error envelope).</summary>
        public string Body { get; }

        public bool IsSuccess => Outcome == HttpOutcome.Success;

        HttpResult(HttpOutcome outcome, int statusCode, T value, string error, string body)
        {
            Outcome = outcome;
            StatusCode = statusCode;
            Value = value;
            Error = error;
            Body = body;
        }

        public static HttpResult<T> Ok(int statusCode, T value) =>
            new HttpResult<T>(HttpOutcome.Success, statusCode, value, null, null);

        public static HttpResult<T> HttpError(int statusCode, string error, string body) =>
            new HttpResult<T>(HttpOutcome.HttpError, statusCode, default, error, body);

        public static HttpResult<T> ConnectionError(string error) =>
            new HttpResult<T>(HttpOutcome.ConnectionError, 0, default, error, null);

        public static HttpResult<T> Canceled() =>
            new HttpResult<T>(HttpOutcome.Canceled, 0, default, "Operation canceled.", null);

        /// <summary>Re-wraps a failure of one payload type as the same failure of another.</summary>
        public static HttpResult<T> FromFailure<TOther>(HttpResult<TOther> other) =>
            new HttpResult<T>(other.Outcome, other.StatusCode, default, other.Error, other.Body);
    }
}

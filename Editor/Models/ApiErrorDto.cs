using System;

namespace Eisenholz.AssetCatalog.Editor.Models
{
    /// <summary>Error envelope returned by the API on 4xx/5xx: <c>{ "error": { "code", "message" } }</c>.</summary>
    [Serializable]
    public sealed class ApiErrorDto
    {
        public ApiError error;
    }

    [Serializable]
    public sealed class ApiError
    {
        public string code;
        public string message;
    }
}

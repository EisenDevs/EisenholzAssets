using Eisenholz.AssetCatalog.Editor.Models;
using UnityEngine.Networking;

namespace Eisenholz.AssetCatalog.Editor.Api
{
    /// <summary>
    /// Builds catalog URLs from the configured base URL (e.g. <c>https://catalog.vps.local/api/v1</c>).
    /// Server-provided paths (thumbnailUrl, downloadUrl) are resolved relative to this base unless they
    /// are already absolute.
    /// </summary>
    public sealed class CatalogEndpoints
    {
        readonly string m_BaseUrl;

        public CatalogEndpoints(string baseUrl) => m_BaseUrl = (baseUrl ?? "").TrimEnd('/');

        public string Health() => $"{m_BaseUrl}/health";

        public string Assets(SearchQuery query)
        {
            var text = UnityWebRequest.EscapeURL(query.Text ?? "");
            var type = UnityWebRequest.EscapeURL(string.IsNullOrEmpty(query.Type) ? "any" : query.Type);
            return $"{m_BaseUrl}/assets?query={text}&type={type}&page={query.Page}&pageSize={query.PageSize}";
        }

        public string Asset(string id) => $"{m_BaseUrl}/assets/{UnityWebRequest.EscapeURL(id)}";

        public string Manifest(string id) => $"{m_BaseUrl}/assets/{UnityWebRequest.EscapeURL(id)}/manifest";

        /// <summary>Resolves a possibly-relative server path against the base URL.</summary>
        public string Resolve(string pathOrUrl)
        {
            if (string.IsNullOrEmpty(pathOrUrl))
                return null;
            if (pathOrUrl.StartsWith("http://") || pathOrUrl.StartsWith("https://"))
                return pathOrUrl;
            return $"{m_BaseUrl}/{pathOrUrl.TrimStart('/')}";
        }
    }
}

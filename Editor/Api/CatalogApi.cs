using System.Threading;
using System.Threading.Tasks;
using Eisenholz.AssetCatalog.Editor.Http;
using Eisenholz.AssetCatalog.Editor.Json;
using Eisenholz.AssetCatalog.Editor.Models;

namespace Eisenholz.AssetCatalog.Editor.Api
{
    /// <summary>Default <see cref="ICatalogApi"/>: fetches JSON over <see cref="IHttpClient"/> and deserializes it.</summary>
    public sealed class CatalogApi : ICatalogApi
    {
        readonly IHttpClient m_Http;
        readonly IJsonSerializer m_Json;
        readonly CatalogEndpoints m_Endpoints;

        public CatalogApi(IHttpClient http, IJsonSerializer json, CatalogEndpoints endpoints)
        {
            m_Http = http;
            m_Json = json;
            m_Endpoints = endpoints;
        }

        public Task<HttpResult<HealthDto>> GetHealthAsync(CancellationToken ct) =>
            GetJsonAsync<HealthDto>(m_Endpoints.Health(), ct);

        public Task<HttpResult<CatalogListResponseDto>> SearchAsync(SearchQuery query, CancellationToken ct) =>
            GetJsonAsync<CatalogListResponseDto>(m_Endpoints.Assets(query), ct);

        public Task<HttpResult<AssetDetailDto>> GetAssetAsync(string id, CancellationToken ct) =>
            GetJsonAsync<AssetDetailDto>(m_Endpoints.Asset(id), ct);

        public Task<HttpResult<AssetManifestDto>> GetManifestAsync(string id, CancellationToken ct) =>
            GetJsonAsync<AssetManifestDto>(m_Endpoints.Manifest(id), ct);

        async Task<HttpResult<T>> GetJsonAsync<T>(string url, CancellationToken ct) where T : class
        {
            var raw = await m_Http.GetTextAsync(url, ct);
            if (!raw.IsSuccess)
                return HttpResult<T>.FromFailure(raw);

            var value = m_Json.Deserialize<T>(raw.Value);
            if (value == null)
                return HttpResult<T>.HttpError(raw.StatusCode, "Malformed or empty JSON response.", raw.Value);

            return HttpResult<T>.Ok(raw.StatusCode, value);
        }
    }
}

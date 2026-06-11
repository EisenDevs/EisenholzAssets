using System.Threading;
using System.Threading.Tasks;
using Eisenholz.AssetCatalog.Editor.Http;
using Eisenholz.AssetCatalog.Editor.Models;

namespace Eisenholz.AssetCatalog.Editor.Api
{
    /// <summary>Typed view over the catalog HTTP API. Maps endpoints to DTOs.</summary>
    public interface ICatalogApi
    {
        Task<HttpResult<HealthDto>> GetHealthAsync(CancellationToken ct);
        Task<HttpResult<CatalogListResponseDto>> SearchAsync(SearchQuery query, CancellationToken ct);
        Task<HttpResult<AssetDetailDto>> GetAssetAsync(string id, CancellationToken ct);
        Task<HttpResult<AssetManifestDto>> GetManifestAsync(string id, CancellationToken ct);
    }
}

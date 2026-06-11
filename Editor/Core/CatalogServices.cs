using Eisenholz.AssetCatalog.Editor.Api;
using Eisenholz.AssetCatalog.Editor.Http;
using Eisenholz.AssetCatalog.Editor.Import;
using Eisenholz.AssetCatalog.Editor.Json;

namespace Eisenholz.AssetCatalog.Editor.Core
{
    /// <summary>
    /// Composition root. Builds the catalog services from the current settings. Auth is the no-op
    /// provider today; swap in a real <see cref="IAuthProvider"/> here when auth is added.
    /// </summary>
    public static class CatalogServices
    {
        public static CatalogEndpoints CreateEndpoints(AssetCatalogSettings settings) =>
            new CatalogEndpoints(settings.CatalogBaseUrl);

        public static IHttpClient CreateHttpClient(AssetCatalogSettings settings) =>
            new CatalogHttpClient(new NullAuthProvider(), settings.RequestTimeoutSeconds);

        public static ICatalogApi CreateApi(AssetCatalogSettings settings) =>
            new CatalogApi(CreateHttpClient(settings), new JsonUtilitySerializer(), CreateEndpoints(settings));

        public static AssetInstaller CreateInstaller(AssetCatalogSettings settings)
        {
            var http = CreateHttpClient(settings);
            var endpoints = CreateEndpoints(settings);
            var api = new CatalogApi(http, new JsonUtilitySerializer(), endpoints);
            return new AssetInstaller(api, http, endpoints, AssetImporterRegistry.CreateDefault(), settings.DefaultImportPath);
        }
    }
}

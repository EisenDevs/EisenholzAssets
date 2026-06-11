using System.Threading;
using System.Threading.Tasks;
using Eisenholz.AssetCatalog.Editor.Api;
using Eisenholz.AssetCatalog.Editor.Http;
using Eisenholz.AssetCatalog.Editor.Json;
using Eisenholz.AssetCatalog.Editor.Models;
using NUnit.Framework;

namespace Eisenholz.AssetCatalog.Tests
{
    public sealed class CatalogApiTests
    {
        const string k_Base = "https://catalog.test/api/v1";

        static CatalogApi MakeApi(FakeHttpClient http) =>
            new CatalogApi(http, new JsonUtilitySerializer(), new CatalogEndpoints(k_Base));

        [Test]
        public async Task SearchAsync_ParsesWrappedList()
        {
            var http = new FakeHttpClient();
            http.SetText(
                $"{k_Base}/assets?query=oak&type=model&page=0&pageSize=50",
                "{\"items\":[{\"id\":\"tree_oak_01\",\"name\":\"Oak\",\"type\":\"model\",\"sizeBytes\":1024}],\"total\":1,\"page\":0,\"pageSize\":50}");

            var query = new SearchQuery { Text = "oak", Type = "model", Page = 0, PageSize = 50 };
            var result = await MakeApi(http).SearchAsync(query, CancellationToken.None);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Value.total);
            Assert.AreEqual(1, result.Value.items.Length);
            Assert.AreEqual("tree_oak_01", result.Value.items[0].id);
            Assert.AreEqual(1024, result.Value.items[0].sizeBytes);
        }

        [Test]
        public async Task GetManifestAsync_ParsesFormatAndFiles()
        {
            var http = new FakeHttpClient();
            http.SetText(
                $"{k_Base}/assets/tree_oak_01/manifest",
                "{\"id\":\"tree_oak_01\",\"format\":\"zip-meta\",\"downloadUrl\":\"/assets/tree_oak_01/download\"," +
                "\"suggestedPath\":\"Assets/Eisenholz/Trees\",\"files\":[{\"relativePath\":\"Trees/Oak.fbx\",\"sizeBytes\":4000}]}");

            var result = await MakeApi(http).GetManifestAsync("tree_oak_01", CancellationToken.None);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("zip-meta", result.Value.format);
            Assert.AreEqual("Assets/Eisenholz/Trees", result.Value.suggestedPath);
            Assert.AreEqual(1, result.Value.files.Length);
            Assert.AreEqual("Trees/Oak.fbx", result.Value.files[0].relativePath);
        }

        [Test]
        public async Task GetAssetAsync_PropagatesHttpFailure()
        {
            var http = new FakeHttpClient();
            http.SetFailure($"{k_Base}/assets/missing",
                HttpResult<string>.HttpError(404, "not found", "{\"error\":{\"code\":\"not_found\"}}"));

            var result = await MakeApi(http).GetAssetAsync("missing", CancellationToken.None);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(HttpOutcome.HttpError, result.Outcome);
            Assert.AreEqual(404, result.StatusCode);
        }

        [Test]
        public async Task GetHealthAsync_TreatsMalformedJsonAsFailure()
        {
            var http = new FakeHttpClient();
            http.SetText($"{k_Base}/health", "this is not json");

            var result = await MakeApi(http).GetHealthAsync(CancellationToken.None);

            Assert.IsFalse(result.IsSuccess);
        }
    }
}

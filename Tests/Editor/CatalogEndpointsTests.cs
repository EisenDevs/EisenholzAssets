using Eisenholz.AssetCatalog.Editor.Api;
using Eisenholz.AssetCatalog.Editor.Models;
using NUnit.Framework;

namespace Eisenholz.AssetCatalog.Tests
{
    public sealed class CatalogEndpointsTests
    {
        const string k_Base = "https://catalog.test/api/v1";

        [Test]
        public void Assets_BuildsEncodedQuery()
        {
            var endpoints = new CatalogEndpoints(k_Base);
            var url = endpoints.Assets(new SearchQuery { Text = "oak tree", Type = "model", Page = 2, PageSize = 25 });

            StringAssert.StartsWith($"{k_Base}/assets?", url);
            StringAssert.Contains("type=model", url);
            StringAssert.Contains("page=2", url);
            StringAssert.Contains("pageSize=25", url);
            StringAssert.DoesNotContain("oak tree", url); // space must be encoded
        }

        [Test]
        public void TrailingSlashOnBaseIsNormalized()
        {
            var endpoints = new CatalogEndpoints(k_Base + "/");
            Assert.AreEqual($"{k_Base}/health", endpoints.Health());
        }

        [Test]
        public void Resolve_RelativePath_CombinesWithBase()
        {
            var endpoints = new CatalogEndpoints(k_Base);
            Assert.AreEqual($"{k_Base}/assets/x/thumb.png", endpoints.Resolve("/assets/x/thumb.png"));
        }

        [Test]
        public void Resolve_AbsoluteUrl_IsReturnedUnchanged()
        {
            var endpoints = new CatalogEndpoints(k_Base);
            const string absolute = "https://cdn.test/x/thumb.png";
            Assert.AreEqual(absolute, endpoints.Resolve(absolute));
        }
    }
}

using Eisenholz.AssetCatalog.Editor.Json;
using Eisenholz.AssetCatalog.Editor.Models;
using NUnit.Framework;

namespace Eisenholz.AssetCatalog.Tests
{
    public sealed class JsonSerializerTests
    {
        readonly IJsonSerializer m_Json = new JsonUtilitySerializer();

        [Test]
        public void Deserialize_KeyValueMetadataArray()
        {
            const string json =
                "{\"id\":\"a\",\"metadata\":[{\"key\":\"author\",\"value\":\"jane\"},{\"key\":\"polycount\",\"value\":\"4200\"}]}";

            var detail = m_Json.Deserialize<AssetDetailDto>(json);

            Assert.IsNotNull(detail);
            Assert.AreEqual(2, detail.metadata.Length);
            Assert.AreEqual("author", detail.metadata[0].key);
            Assert.AreEqual("jane", detail.metadata[0].value);
        }

        [Test]
        public void Deserialize_NullOrEmpty_ReturnsDefault()
        {
            Assert.IsNull(m_Json.Deserialize<HealthDto>(null));
            Assert.IsNull(m_Json.Deserialize<HealthDto>(""));
        }

        [Test]
        public void Deserialize_Malformed_ReturnsNullNotThrow()
        {
            Assert.DoesNotThrow(() => m_Json.Deserialize<HealthDto>("{ not valid json"));
        }
    }
}

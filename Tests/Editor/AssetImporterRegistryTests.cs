using Eisenholz.AssetCatalog.Editor.Import;
using Eisenholz.AssetCatalog.Editor.Models;
using NUnit.Framework;

namespace Eisenholz.AssetCatalog.Tests
{
    public sealed class AssetImporterRegistryTests
    {
        readonly AssetImporterRegistry m_Registry = AssetImporterRegistry.CreateDefault();

        static AssetManifestDto Manifest(string format) => new AssetManifestDto { format = format };

        [Test]
        public void Find_UnityPackage()
        {
            Assert.IsInstanceOf<UnityPackageImporter>(m_Registry.Find(Manifest("unitypackage")));
        }

        [Test]
        public void Find_ZipMeta()
        {
            Assert.IsInstanceOf<ZipWithMetaImporter>(m_Registry.Find(Manifest("zip-meta")));
        }

        [Test]
        public void Find_Raw()
        {
            Assert.IsInstanceOf<RawFileImporter>(m_Registry.Find(Manifest("raw")));
        }

        [Test]
        public void Find_IsCaseInsensitive()
        {
            Assert.IsInstanceOf<UnityPackageImporter>(m_Registry.Find(Manifest("UnityPackage")));
        }

        [Test]
        public void Find_UnknownFormat_ReturnsNull()
        {
            Assert.IsNull(m_Registry.Find(Manifest("tarball")));
        }
    }
}

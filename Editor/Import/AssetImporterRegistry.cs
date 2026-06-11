using System.Collections.Generic;
using Eisenholz.AssetCatalog.Editor.Models;

namespace Eisenholz.AssetCatalog.Editor.Import
{
    /// <summary>Ordered list of importers; the first that <see cref="IAssetImporter.CanHandle"/> wins.</summary>
    public sealed class AssetImporterRegistry
    {
        readonly List<IAssetImporter> m_Importers;

        public AssetImporterRegistry(IEnumerable<IAssetImporter> importers) =>
            m_Importers = new List<IAssetImporter>(importers);

        public IAssetImporter Find(AssetManifestDto manifest)
        {
            foreach (var importer in m_Importers)
            {
                if (importer.CanHandle(manifest))
                    return importer;
            }

            return null;
        }

        public static AssetImporterRegistry CreateDefault() =>
            new AssetImporterRegistry(new IAssetImporter[]
            {
                new UnityPackageImporter(),
                new ZipWithMetaImporter(),
                new RawFileImporter()
            });
    }
}

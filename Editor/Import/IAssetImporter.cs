using System;
using System.Threading;
using System.Threading.Tasks;
using Eisenholz.AssetCatalog.Editor.Models;

namespace Eisenholz.AssetCatalog.Editor.Import
{
    /// <summary>
    /// Strategy for turning a downloaded payload into project assets. Add a new delivery format by
    /// writing a new implementation and registering it — nothing else changes.
    /// </summary>
    public interface IAssetImporter
    {
        /// <summary>True if this importer understands the manifest's <c>format</c>.</summary>
        bool CanHandle(AssetManifestDto manifest);

        Task<ImportResult> ImportAsync(ImportContext context, IProgress<float> progress, CancellationToken ct);
    }
}

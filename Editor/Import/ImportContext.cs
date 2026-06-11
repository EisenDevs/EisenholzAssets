using Eisenholz.AssetCatalog.Editor.Models;

namespace Eisenholz.AssetCatalog.Editor.Import
{
    /// <summary>Everything an <see cref="IAssetImporter"/> needs to place a downloaded asset.</summary>
    public sealed class ImportContext
    {
        /// <summary>Path to the downloaded payload on disk (under Library/, never under Assets/).</summary>
        public string DownloadedFilePath;

        /// <summary>The manifest describing the asset (format, files, suggested path…).</summary>
        public AssetManifestDto Manifest;

        /// <summary>Project-relative destination directory (validated to be under Assets/).</summary>
        public string TargetDirectory;

        /// <summary>When false, existing files are left untouched instead of being overwritten.</summary>
        public bool Overwrite = true;
    }
}

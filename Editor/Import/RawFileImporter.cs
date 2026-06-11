using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eisenholz.AssetCatalog.Editor.Models;
using Eisenholz.AssetCatalog.Editor.Utils;
using UnityEditor;

namespace Eisenholz.AssetCatalog.Editor.Import
{
    /// <summary>
    /// Copies a single downloaded file into the target folder and lets Unity generate a fresh .meta/GUID.
    /// The destination name comes from the manifest's first file entry (so it may include subfolders),
    /// falling back to the download URL leaf or the asset id.
    /// </summary>
    public sealed class RawFileImporter : IAssetImporter
    {
        public bool CanHandle(AssetManifestDto manifest) =>
            string.Equals(manifest?.format, "raw", StringComparison.OrdinalIgnoreCase);

        public Task<ImportResult> ImportAsync(ImportContext context, IProgress<float> progress, CancellationToken ct)
        {
            var targetAbs = Path.GetFullPath(context.TargetDirectory);
            var relativeName = ResolveRelativeName(context.Manifest).Replace('\\', '/');

            var dest = Path.GetFullPath(Path.Combine(targetAbs, relativeName));
            if (!ProjectPaths.IsContained(targetAbs, dest))
                return Task.FromResult(ImportResult.Fail($"Rejected unsafe file path: '{relativeName}'."));

            if (File.Exists(dest) && !context.Overwrite)
                return Task.FromResult(ImportResult.Fail($"'{relativeName}' already exists (overwrite disabled)."));

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(dest));
                progress?.Report(0.9f);
                File.Copy(context.DownloadedFilePath, dest, overwrite: true);
            }
            catch (Exception e)
            {
                return Task.FromResult(ImportResult.Fail($"Copy failed: {e.Message}"));
            }

            AssetDatabase.Refresh();
            progress?.Report(1f);

            var projectRelative = ProjectPaths.ToProjectRelative(dest);
            return Task.FromResult(ImportResult.Ok(new[] { projectRelative }, $"Imported {projectRelative}."));
        }

        static string ResolveRelativeName(AssetManifestDto manifest)
        {
            if (manifest.files != null && manifest.files.Length > 0 &&
                !string.IsNullOrEmpty(manifest.files[0].relativePath))
                return manifest.files[0].relativePath;

            var url = manifest.downloadUrl ?? "";
            var withoutQuery = url.Split('?')[0];
            var leaf = withoutQuery.Substring(withoutQuery.LastIndexOf('/') + 1);
            return string.IsNullOrEmpty(leaf) ? (manifest.id ?? "asset") : leaf;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eisenholz.AssetCatalog.Editor.Models;
using Eisenholz.AssetCatalog.Editor.Utils;
using UnityEditor;

namespace Eisenholz.AssetCatalog.Editor.Import
{
    /// <summary>
    /// Extracts a zip that contains asset files AND their <c>.meta</c> siblings into the target folder,
    /// preserving the shipped GUIDs so cross-machine references stay intact. A single Refresh runs after
    /// all files are written.
    /// </summary>
    public sealed class ZipWithMetaImporter : IAssetImporter
    {
        public bool CanHandle(AssetManifestDto manifest) =>
            string.Equals(manifest?.format, "zip-meta", StringComparison.OrdinalIgnoreCase);

        public Task<ImportResult> ImportAsync(ImportContext context, IProgress<float> progress, CancellationToken ct)
        {
            var targetAbs = Path.GetFullPath(context.TargetDirectory);
            Directory.CreateDirectory(targetAbs);

            progress?.Report(0.9f);
            var extraction = SafeExtractor.ExtractZip(context.DownloadedFilePath, targetAbs, context.Overwrite);

            // Files were written outside the AssetDatabase API, so a single Refresh imports them all.
            AssetDatabase.Refresh();
            progress?.Report(1f);

            if (!extraction.Success)
                return Task.FromResult(extraction);

            var missingMeta = CountMissingMeta(extraction.ImportedPaths);
            var relative = ToProjectRelative(extraction.ImportedPaths);

            var message = $"Imported {relative.Count} file(s) into {context.TargetDirectory}.";
            if (missingMeta > 0)
                message += $" Warning: {missingMeta} asset(s) had no .meta — GUIDs were regenerated.";

            return Task.FromResult(ImportResult.Ok(relative, message));
        }

        static int CountMissingMeta(IReadOnlyList<string> absolutePaths)
        {
            var set = new HashSet<string>(absolutePaths, StringComparer.Ordinal);
            var missing = 0;
            foreach (var path in absolutePaths)
            {
                if (path.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!set.Contains(path + ".meta"))
                    missing++;
            }

            return missing;
        }

        static List<string> ToProjectRelative(IReadOnlyList<string> absolutePaths)
        {
            var list = new List<string>(absolutePaths.Count);
            foreach (var path in absolutePaths)
                list.Add(ProjectPaths.ToProjectRelative(path));
            return list;
        }
    }
}

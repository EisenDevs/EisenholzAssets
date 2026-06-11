using System;
using System.Threading;
using System.Threading.Tasks;
using Eisenholz.AssetCatalog.Editor.Models;
using UnityEditor;

namespace Eisenholz.AssetCatalog.Editor.Import
{
    /// <summary>
    /// Imports a <c>.unitypackage</c> via <see cref="AssetDatabase.ImportPackage"/>. The package carries
    /// its own paths and GUIDs, so the target directory and Refresh are handled by Unity itself.
    /// </summary>
    public sealed class UnityPackageImporter : IAssetImporter
    {
        public bool CanHandle(AssetManifestDto manifest) =>
            string.Equals(manifest?.format, "unitypackage", StringComparison.OrdinalIgnoreCase);

        public Task<ImportResult> ImportAsync(ImportContext context, IProgress<float> progress, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<ImportResult>();

            AssetDatabase.ImportPackageCallback onCompleted = null;
            AssetDatabase.ImportPackageFailedCallback onFailed = null;

            void Unsubscribe()
            {
                AssetDatabase.importPackageCompleted -= onCompleted;
                AssetDatabase.importPackageFailed -= onFailed;
            }

            onCompleted = packageName =>
            {
                Unsubscribe();
                progress?.Report(1f);
                tcs.TrySetResult(ImportResult.Ok(Array.Empty<string>(), $"Imported package '{packageName}'."));
            };
            onFailed = (packageName, error) =>
            {
                Unsubscribe();
                tcs.TrySetResult(ImportResult.Fail($"Package import failed: {error}"));
            };

            AssetDatabase.importPackageCompleted += onCompleted;
            AssetDatabase.importPackageFailed += onFailed;

            progress?.Report(0.9f);
            try
            {
                AssetDatabase.ImportPackage(context.DownloadedFilePath, interactive: false);
            }
            catch (Exception e)
            {
                Unsubscribe();
                tcs.TrySetResult(ImportResult.Fail($"ImportPackage threw: {e.Message}"));
            }

            return tcs.Task;
        }
    }
}

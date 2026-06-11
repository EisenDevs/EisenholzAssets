using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eisenholz.AssetCatalog.Editor.Api;
using Eisenholz.AssetCatalog.Editor.Http;
using Eisenholz.AssetCatalog.Editor.Utils;

namespace Eisenholz.AssetCatalog.Editor.Import
{
    /// <summary>
    /// Orchestrates a full install: fetch manifest → pick importer → download to a temp file →
    /// verify checksum → import → clean up. Reports overall progress in [0,1].
    /// </summary>
    public sealed class AssetInstaller
    {
        /// <summary>Temp staging dir for downloads. Machine-local (under Library/), never imported.</summary>
        public const string DownloadDirectory = "Library/AssetCatalog/Downloads";

        readonly ICatalogApi m_Api;
        readonly IHttpClient m_Http;
        readonly CatalogEndpoints m_Endpoints;
        readonly AssetImporterRegistry m_Registry;
        readonly string m_DefaultImportPath;

        public AssetInstaller(ICatalogApi api, IHttpClient http, CatalogEndpoints endpoints,
            AssetImporterRegistry registry, string defaultImportPath)
        {
            m_Api = api;
            m_Http = http;
            m_Endpoints = endpoints;
            m_Registry = registry;
            m_DefaultImportPath = string.IsNullOrEmpty(defaultImportPath) ? "Assets/Eisenholz" : defaultImportPath;
        }

        /// <summary>
        /// Removes leftover temp downloads. The per-install finally-block cleanup does not run when a
        /// domain reload (script recompile) tears down an in-flight download, so we also sweep the whole
        /// staging dir on editor load. Everything here is disposable temp data, safe to delete wholesale.
        /// </summary>
        public static void CleanStaleDownloads()
        {
            try
            {
                if (Directory.Exists(DownloadDirectory))
                    Directory.Delete(DownloadDirectory, true);
            }
            catch
            {
                // Best-effort; a locked/missing temp dir is harmless and gets recreated on next install.
            }
        }

        public async Task<ImportResult> InstallAsync(string assetId, IProgress<float> progress, CancellationToken ct)
        {
            var manifestResult = await m_Api.GetManifestAsync(assetId, ct);
            if (ct.IsCancellationRequested)
                return ImportResult.Fail("Canceled.");
            if (!manifestResult.IsSuccess || manifestResult.Value == null)
                return ImportResult.Fail($"Could not fetch manifest: {manifestResult.Error}");

            var manifest = manifestResult.Value;

            var importer = m_Registry.Find(manifest);
            if (importer == null)
                return ImportResult.Fail($"No importer for format '{manifest.format}'.");

            var targetDir = ResolveTargetDirectory(manifest);
            if (targetDir == null)
                return ImportResult.Fail("Import target resolved outside Assets/. Aborting.");

            var url = m_Endpoints.Resolve(manifest.downloadUrl);
            if (string.IsNullOrEmpty(url))
                return ImportResult.Fail("Manifest has no downloadUrl.");

            Directory.CreateDirectory(DownloadDirectory);
            var tempPath = Path.Combine(DownloadDirectory,
                FormatUtils.SafeFileName($"{manifest.id}_{manifest.version}") + TempExtension(manifest.format));

            try
            {
                // Download counts for 0..0.85 of overall progress.
                var dlProgress = new ActionProgress<HttpProgress>(p => progress?.Report(p.Fraction * 0.85f));
                var download = await m_Http.DownloadToFileAsync(url, tempPath, dlProgress, ct);

                if (download.Outcome == HttpOutcome.Canceled || ct.IsCancellationRequested)
                    return ImportResult.Fail("Canceled.");
                if (!download.IsSuccess)
                    return ImportResult.Fail($"Download failed ({download.StatusCode}): {download.Error}");

                if (!string.IsNullOrEmpty(manifest.sha256))
                {
                    var actual = Sha256.OfFile(tempPath);
                    if (!string.Equals(actual, manifest.sha256, StringComparison.OrdinalIgnoreCase))
                        return ImportResult.Fail("Checksum mismatch — download may be corrupt or tampered.");
                }

                var context = new ImportContext
                {
                    DownloadedFilePath = tempPath,
                    Manifest = manifest,
                    TargetDirectory = targetDir,
                    Overwrite = true
                };

                // Import maps to 0.85..1.0.
                var importProgress = new ActionProgress<float>(f => progress?.Report(0.85f + 0.15f * f));
                return await importer.ImportAsync(context, importProgress, ct);
            }
            finally
            {
                try
                {
                    if (File.Exists(tempPath))
                        File.Delete(tempPath);
                }
                catch
                {
                    // Best-effort cleanup of the temp download.
                }
            }
        }

        string ResolveTargetDirectory(Models.AssetManifestDto manifest)
        {
            var path = string.IsNullOrEmpty(manifest.suggestedPath) ? m_DefaultImportPath : manifest.suggestedPath;
            path = path.Replace('\\', '/').TrimEnd('/');
            return ProjectPaths.IsUnderAssets(path) ? path : null;
        }

        static string TempExtension(string format)
        {
            switch ((format ?? "").ToLowerInvariant())
            {
                case "unitypackage": return ".unitypackage";
                case "raw": return ".bin";
                default: return ".zip";
            }
        }
    }
}

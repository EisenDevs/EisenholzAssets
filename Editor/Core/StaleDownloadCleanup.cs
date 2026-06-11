using Eisenholz.AssetCatalog.Editor.Import;
using UnityEditor;

namespace Eisenholz.AssetCatalog.Editor.Core
{
    /// <summary>
    /// Sweeps leftover temp downloads on every editor load / domain reload. An in-flight download is
    /// torn down (not finalized) when scripts recompile, so its finally-block cleanup never runs —
    /// this catches the orphaned temp files the next time the domain comes up.
    /// </summary>
    [InitializeOnLoad]
    static class StaleDownloadCleanup
    {
        static StaleDownloadCleanup()
        {
            AssetInstaller.CleanStaleDownloads();
        }
    }
}

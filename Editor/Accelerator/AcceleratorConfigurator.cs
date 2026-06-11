using UnityEditor;

namespace Eisenholz.AssetCatalog.Editor.Accelerator
{
    /// <summary>
    /// Reads/writes this Editor's project-specific Cache Server (Unity Accelerator) settings via
    /// <see cref="EditorSettings"/>. These live in ProjectSettings/EditorSettings.asset, so applying them
    /// is a committed, team-wide pointer. Decoupled from the catalog — the Accelerator is just a cache.
    /// </summary>
    public static class AcceleratorConfigurator
    {
        public static CacheServerMode CurrentMode => EditorSettings.cacheServerMode;

        public static string CurrentEndpoint => EditorSettings.cacheServerEndpoint;

        /// <summary>Points the Editor's Cache Server at host:port and applies it. May trigger a reimport.</summary>
        public static void Apply(string host, int port)
        {
            EditorSettings.cacheServerMode = CacheServerMode.Enabled;
            EditorSettings.cacheServerEndpoint = $"{host}:{port}";
            AssetDatabase.RefreshSettings();
        }

        public static void Disable()
        {
            EditorSettings.cacheServerMode = CacheServerMode.Disabled;
            AssetDatabase.RefreshSettings();
        }
    }
}

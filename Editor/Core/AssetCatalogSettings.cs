using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Eisenholz.AssetCatalog.Editor.Core
{
    /// <summary>
    /// Project-level settings for the Asset Catalog tool. Persisted under
    /// <c>ProjectSettings/AssetCatalogSettings.asset</c> so the server URL is a shared,
    /// committed, team value (not per-machine). Per-user / secret values (e.g. a future
    /// auth token) must NOT live here — use EditorPrefs for those.
    /// </summary>
    public sealed class AssetCatalogSettings : ScriptableObject
    {
        internal const string k_SettingsPath = "ProjectSettings/AssetCatalogSettings.asset";

        [Header("Catalog Service")]
        [Tooltip("Base URL of the custom Asset Catalog HTTP API, e.g. https://catalog.vps.local/api/v1")]
        public string CatalogBaseUrl = "https://catalog.vps.local/api/v1";

        [Tooltip("Default folder (under Assets/) where downloads are imported when a manifest has no suggested path.")]
        public string DefaultImportPath = "Assets/Eisenholz";

        [Tooltip("HTTP request timeout, in seconds.")]
        public int RequestTimeoutSeconds = 30;

        [Tooltip("Number of catalog entries fetched per page.")]
        public int PageSize = 50;

        [Header("Unity Accelerator (optional)")]
        [Tooltip("Accelerator host, e.g. accelerator.vps.local. Leave empty to skip.")]
        public string AcceleratorHost = "";

        [Tooltip("Accelerator port (Unity default is 10080).")]
        public int AcceleratorPort = 10080;

        static AssetCatalogSettings s_Instance;

        /// <summary>Loads the settings asset from ProjectSettings, creating defaults on first use.</summary>
        public static AssetCatalogSettings GetOrCreate()
        {
            if (s_Instance != null)
                return s_Instance;

            var loaded = InternalEditorUtility.LoadSerializedFileAndForget(k_SettingsPath);
            foreach (var obj in loaded)
            {
                s_Instance = obj as AssetCatalogSettings;
                if (s_Instance != null)
                    break;
            }

            if (s_Instance == null)
            {
                s_Instance = CreateInstance<AssetCatalogSettings>();
                s_Instance.name = "AssetCatalogSettings";
                Save();
            }

            return s_Instance;
        }

        /// <summary>Persists the in-memory settings back to the ProjectSettings file.</summary>
        public static void Save()
        {
            if (s_Instance == null)
                return;

            InternalEditorUtility.SaveToSerializedFileAndForget(
                new Object[] { s_Instance }, k_SettingsPath, allowTextSerialization: true);
        }
    }
}

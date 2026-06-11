using UnityEngine;

namespace Eisenholz.AssetCatalog.Editor.Core
{
    /// <summary>Thin, prefixed logging wrapper so all tool output is easy to spot and filter.</summary>
    static class CatalogLog
    {
        const string k_Prefix = "<b>[Asset Catalog]</b> ";

        public static void Info(string message) => Debug.Log(k_Prefix + message);
        public static void Warn(string message) => Debug.LogWarning(k_Prefix + message);
        public static void Error(string message) => Debug.LogError(k_Prefix + message);
    }
}

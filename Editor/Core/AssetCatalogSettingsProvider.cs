using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Eisenholz.AssetCatalog.Editor.Core
{
    /// <summary>
    /// Registers the Asset Catalog settings page under
    /// <b>Project Settings ▸ Eisenholz ▸ Asset Catalog</b>.
    /// </summary>
    static class AssetCatalogSettingsProvider
    {
        internal const string k_ProjectPath = "Project/Eisenholz/Asset Catalog";

        [SettingsProvider]
        public static SettingsProvider Create()
        {
            return new SettingsProvider(k_ProjectPath, SettingsScope.Project)
            {
                label = "Asset Catalog",
                guiHandler = _ => DrawGui(),
                keywords = new HashSet<string>(new[]
                {
                    "asset", "catalog", "accelerator", "eisenholz", "download", "vps"
                })
            };
        }

        static void DrawGui()
        {
            var settings = AssetCatalogSettings.GetOrCreate();
            var so = new SerializedObject(settings);
            so.Update();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("Catalog Service", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(so.FindProperty(nameof(AssetCatalogSettings.CatalogBaseUrl)), new GUIContent("Base URL"));
            EditorGUILayout.PropertyField(so.FindProperty(nameof(AssetCatalogSettings.DefaultImportPath)), new GUIContent("Default Import Path"));
            EditorGUILayout.PropertyField(so.FindProperty(nameof(AssetCatalogSettings.RequestTimeoutSeconds)), new GUIContent("Request Timeout (s)"));
            EditorGUILayout.PropertyField(so.FindProperty(nameof(AssetCatalogSettings.PageSize)), new GUIContent("Page Size"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Unity Accelerator", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(so.FindProperty(nameof(AssetCatalogSettings.AcceleratorHost)), new GUIContent("Host"));
            EditorGUILayout.PropertyField(so.FindProperty(nameof(AssetCatalogSettings.AcceleratorPort)), new GUIContent("Port"));

            if (EditorGUI.EndChangeCheck())
            {
                so.ApplyModifiedPropertiesWithoutUndo();
                AssetCatalogSettings.Save();
            }
        }
    }
}

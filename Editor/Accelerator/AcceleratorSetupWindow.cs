using Eisenholz.AssetCatalog.Editor.Core;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Eisenholz.AssetCatalog.Editor.Accelerator
{
    /// <summary>
    /// Onboarding helper to point this Editor's Cache Server at the team's Unity Accelerator and to
    /// probe reachability. Fully separate from the catalog browser.
    /// </summary>
    public sealed class AcceleratorSetupWindow : EditorWindow
    {
        TextField m_Host;
        IntegerField m_Port;
        Label m_Current;
        Label m_Status;

        [MenuItem("Window/Eisenholz/Unity Accelerator")]
        public static void Open()
        {
            var window = GetWindow<AcceleratorSetupWindow>();
            window.titleContent = new GUIContent("Unity Accelerator");
            window.minSize = new Vector2(440f, 300f);
            window.Show();
        }

        void CreateGUI()
        {
            var settings = AssetCatalogSettings.GetOrCreate();

            var root = rootVisualElement;
            root.style.paddingLeft = 12f;
            root.style.paddingRight = 12f;
            root.style.paddingTop = 10f;

            root.Add(new Label("Unity Accelerator Setup")
            {
                style = { fontSize = 15f, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 4f }
            });

            root.Add(new HelpBox(
                "Points this Editor's Cache Server at your Accelerator so imports use the shared cache. " +
                "Applying changes the project Cache Server setting and can trigger a one-time reimport.",
                HelpBoxMessageType.Info)
            { style = { marginBottom = 8f } });

            m_Current = new Label { style = { color = new Color(0.6f, 0.65f, 0.72f), marginBottom = 8f } };
            root.Add(m_Current);

            m_Host = new TextField("Host") { value = settings.AcceleratorHost };
            m_Host.RegisterValueChangedCallback(e =>
            {
                settings.AcceleratorHost = e.newValue;
                AssetCatalogSettings.Save();
            });
            root.Add(m_Host);

            m_Port = new IntegerField("Port") { value = settings.AcceleratorPort };
            m_Port.RegisterValueChangedCallback(e =>
            {
                settings.AcceleratorPort = e.newValue;
                AssetCatalogSettings.Save();
            });
            root.Add(m_Port);

            var buttons = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 10f } };
            buttons.Add(new Button(TestReachability) { text = "Test Reachability" });
            buttons.Add(new Button(ApplyToEditor) { text = "Apply to Editor", style = { marginLeft = 6f } });
            buttons.Add(new Button(DisableCacheServer) { text = "Disable", style = { marginLeft = 6f } });
            root.Add(buttons);

            m_Status = new Label { style = { marginTop = 8f, whiteSpace = WhiteSpace.Normal } };
            root.Add(m_Status);

            RefreshCurrent();
        }

        void RefreshCurrent()
        {
            var endpoint = string.IsNullOrEmpty(AcceleratorConfigurator.CurrentEndpoint)
                ? "(none)"
                : AcceleratorConfigurator.CurrentEndpoint;
            m_Current.text = $"Current Editor Cache Server: {AcceleratorConfigurator.CurrentMode} — {endpoint}";
        }

        async void TestReachability()
        {
            var settings = AssetCatalogSettings.GetOrCreate();
            SetStatus("Testing…", false);
            var result = await AcceleratorHealthCheck.ProbeAsync(settings.AcceleratorHost, settings.AcceleratorPort);
            SetStatus((result.Ok ? "✓ " : "✗ ") + result.Message, !result.Ok);
        }

        void ApplyToEditor()
        {
            var settings = AssetCatalogSettings.GetOrCreate();
            if (string.IsNullOrEmpty(settings.AcceleratorHost))
            {
                SetStatus("✗ Set a host first.", true);
                return;
            }

            var apply = EditorUtility.DisplayDialog(
                "Apply Cache Server",
                $"Point this Editor's Cache Server at {settings.AcceleratorHost}:{settings.AcceleratorPort}?\n\n" +
                "This can trigger a one-time project reimport.",
                "Apply", "Cancel");
            if (!apply)
                return;

            AcceleratorConfigurator.Apply(settings.AcceleratorHost, settings.AcceleratorPort);
            RefreshCurrent();
            SetStatus("✓ Applied. A reimport may run now.", false);
        }

        void DisableCacheServer()
        {
            var disable = EditorUtility.DisplayDialog(
                "Disable Cache Server", "Disable this Editor's Cache Server?", "Disable", "Cancel");
            if (!disable)
                return;

            AcceleratorConfigurator.Disable();
            RefreshCurrent();
            SetStatus("Cache Server disabled.", false);
        }

        void SetStatus(string message, bool isError)
        {
            if (m_Status == null)
                return;
            m_Status.text = message;
            m_Status.style.color = isError ? new Color(0.95f, 0.5f, 0.45f) : new Color(0.4f, 0.85f, 0.5f);
        }
    }
}

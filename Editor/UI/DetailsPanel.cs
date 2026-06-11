using System;
using System.Threading;
using Eisenholz.AssetCatalog.Editor.Api;
using Eisenholz.AssetCatalog.Editor.Models;
using Eisenholz.AssetCatalog.Editor.Thumbnails;
using Eisenholz.AssetCatalog.Editor.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Eisenholz.AssetCatalog.Editor.UI
{
    /// <summary>Right-hand panel: the selected asset's detail plus the Download &amp; Import action.</summary>
    public sealed class DetailsPanel : VisualElement
    {
        readonly Action<CatalogEntryDto> m_OnDownload;

        readonly Image m_Preview;
        readonly Label m_Name;
        readonly Label m_Meta;
        readonly Label m_Description;
        readonly VisualElement m_MetadataList;
        readonly Button m_DownloadButton;
        readonly ProgressBar m_Progress;
        readonly Label m_DownloadStatus;
        readonly Label m_Empty;
        readonly VisualElement m_Content;

        CatalogEntryDto m_Current;

        public DetailsPanel(Action<CatalogEntryDto> onDownload)
        {
            m_OnDownload = onDownload;

            style.width = 270f;
            style.flexShrink = 0f;
            style.paddingLeft = 10f;
            style.borderLeftWidth = 1f;
            style.borderLeftColor = new Color(0.18f, 0.19f, 0.22f);

            m_Empty = new Label("Select an asset to see its details.")
            {
                style = { color = new Color(0.55f, 0.6f, 0.68f), whiteSpace = WhiteSpace.Normal, marginTop = 8f }
            };
            Add(m_Empty);

            m_Content = new VisualElement { style = { display = DisplayStyle.None } };
            Add(m_Content);

            m_Preview = new Image
            {
                scaleMode = ScaleMode.ScaleToFit,
                style = { height = 160f, backgroundColor = new Color(0.12f, 0.13f, 0.15f), marginBottom = 8f }
            };
            m_Content.Add(m_Preview);

            m_Name = new Label { style = { fontSize = 14f, unityFontStyleAndWeight = FontStyle.Bold, whiteSpace = WhiteSpace.Normal } };
            m_Content.Add(m_Name);

            m_Meta = new Label { style = { color = new Color(0.6f, 0.65f, 0.72f), marginBottom = 6f } };
            m_Content.Add(m_Meta);

            m_Description = new Label { style = { whiteSpace = WhiteSpace.Normal, marginBottom = 6f } };
            m_Content.Add(m_Description);

            m_MetadataList = new VisualElement();
            m_Content.Add(m_MetadataList);

            m_DownloadButton = new Button(() => { if (m_Current != null) m_OnDownload?.Invoke(m_Current); })
            {
                text = "Download & Import",
                style = { marginTop = 10f }
            };
            m_Content.Add(m_DownloadButton);

            m_Progress = new ProgressBar { lowValue = 0f, highValue = 100f, value = 0f, style = { display = DisplayStyle.None, marginTop = 6f } };
            m_Content.Add(m_Progress);

            m_DownloadStatus = new Label { style = { whiteSpace = WhiteSpace.Normal, marginTop = 4f } };
            m_Content.Add(m_DownloadStatus);
        }

        /// <summary>Resets the panel to the empty state. (Named to avoid hiding VisualElement.Clear().)</summary>
        public void ClearSelection()
        {
            m_Current = null;
            m_Empty.style.display = DisplayStyle.Flex;
            m_Content.style.display = DisplayStyle.None;
            m_Preview.image = null;
        }

        public void SetBusy(bool busy)
        {
            m_DownloadButton.SetEnabled(!busy);
            m_Progress.style.display = busy ? DisplayStyle.Flex : DisplayStyle.None;
            if (busy)
                m_Progress.value = 0f;
        }

        public void ReportProgress(float fraction)
        {
            var pct = Mathf.Clamp01(fraction) * 100f;
            m_Progress.value = pct;
            m_Progress.title = $"{pct:0}%";
        }

        public void SetDownloadStatus(string message, bool isError)
        {
            m_DownloadStatus.text = message;
            m_DownloadStatus.style.color = isError ? new Color(0.95f, 0.5f, 0.45f) : new Color(0.4f, 0.85f, 0.5f);
        }

        public async void Show(CatalogEntryDto entry, ICatalogApi api, ThumbnailService thumbs, CancellationToken ct)
        {
            m_Current = entry;
            m_Empty.style.display = DisplayStyle.None;
            m_Content.style.display = DisplayStyle.Flex;

            m_Name.text = entry.name;
            m_Meta.text = $"{entry.type} · v{entry.version} · {FormatUtils.Bytes(entry.sizeBytes)}";
            m_Description.text = "";
            m_MetadataList.Clear();
            m_DownloadStatus.text = "";
            m_Preview.image = null;

            var thumbKey = $"{entry.id}_{entry.version}";
            var tex = await thumbs.GetAsync(thumbKey, entry.thumbnailUrl, ct);
            if (!ct.IsCancellationRequested && tex != null && m_Current == entry)
                m_Preview.image = tex;

            var detail = await api.GetAssetAsync(entry.id, ct);
            if (ct.IsCancellationRequested || m_Current != entry || !detail.IsSuccess || detail.Value == null)
                return;

            m_Description.text = detail.Value.description ?? "";
            m_MetadataList.Clear();
            if (detail.Value.metadata != null)
            {
                foreach (var kv in detail.Value.metadata)
                {
                    m_MetadataList.Add(new Label($"{kv.key}: {kv.value}")
                    {
                        style = { fontSize = 11f, color = new Color(0.6f, 0.65f, 0.72f) }
                    });
                }
            }
        }
    }
}

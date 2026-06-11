using System.Collections.Generic;
using System.Threading;
using Eisenholz.AssetCatalog.Editor.Api;
using Eisenholz.AssetCatalog.Editor.Core;
using Eisenholz.AssetCatalog.Editor.Models;
using Eisenholz.AssetCatalog.Editor.Thumbnails;
using Eisenholz.AssetCatalog.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Eisenholz.AssetCatalog.Editor.UI
{
    /// <summary>
    /// Main browser window: search/type toolbar, paged thumbnail grid, and a details panel.
    /// Download/import is wired in M4.
    /// </summary>
    public sealed class AssetCatalogWindow : EditorWindow
    {
        ToolbarSearchField m_Search;
        DropdownField m_TypeFilter;
        ScrollView m_Grid;
        DetailsPanel m_Details;
        Label m_Status;
        Label m_PageInfo;
        Button m_Prev;
        Button m_Next;

        ICatalogApi m_Api;
        ThumbnailService m_Thumbs;
        ThumbnailCache m_Cache;

        CancellationTokenSource m_Cts;
        CancellationTokenSource m_DownloadCts;
        IVisualElementScheduledItem m_SearchDebounce;
        CatalogEntryCard m_SelectedCard;
        int m_Page;
        int m_Total;

        [MenuItem("Window/Eisenholz/Asset Catalog")]
        public static void Open()
        {
            var window = GetWindow<AssetCatalogWindow>();
            window.titleContent = new GUIContent("Asset Catalog");
            window.minSize = new Vector2(640f, 420f);
            window.Show();
        }

        void OnDisable()
        {
            CancelInFlight();
            m_DownloadCts?.Cancel();
            m_DownloadCts?.Dispose();
            m_DownloadCts = null;
            m_Cache?.Dispose();
            m_Cache = null;
        }

        void CreateGUI()
        {
            m_Cache = new ThumbnailCache();

            var root = rootVisualElement;
            root.style.paddingLeft = 10f;
            root.style.paddingRight = 10f;
            root.style.paddingTop = 8f;
            root.style.paddingBottom = 8f;

            root.Add(BuildToolbar());

            var body = new VisualElement { style = { flexDirection = FlexDirection.Row, flexGrow = 1f } };
            m_Grid = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1f } };
            m_Grid.contentContainer.style.flexDirection = FlexDirection.Row;
            m_Grid.contentContainer.style.flexWrap = Wrap.Wrap;
            m_Grid.contentContainer.style.alignContent = Align.FlexStart;
            body.Add(m_Grid);

            m_Details = new DetailsPanel(OnDownloadRequested);
            body.Add(m_Details);
            root.Add(body);

            root.Add(BuildFooter());

            Reload();
        }

        VisualElement BuildToolbar()
        {
            var toolbar = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 8f }
            };

            m_Search = new ToolbarSearchField { style = { flexGrow = 1f } };
            m_Search.RegisterValueChangedCallback(_ => OnSearchChanged());
            toolbar.Add(m_Search);

            // Mirrors the REST-contract type taxonomy. "any" maps to no type filter.
            m_TypeFilter = new DropdownField(
                new List<string> { "any", "model", "texture", "script", "audio", "prefab", "material", "shader" }, 0)
            {
                style = { width = 120f, marginLeft = 6f }
            };
            m_TypeFilter.RegisterValueChangedCallback(_ => ReloadFromFirstPage());
            toolbar.Add(m_TypeFilter);

            toolbar.Add(new Button(ReloadFromFirstPage) { text = "Refresh", style = { marginLeft = 6f } });
            toolbar.Add(new Button(
                () => SettingsService.OpenProjectSettings(AssetCatalogSettingsProvider.k_ProjectPath))
            {
                text = "⚙", tooltip = "Settings", style = { marginLeft = 6f }
            });

            return toolbar;
        }

        VisualElement BuildFooter()
        {
            var footer = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginTop = 6f }
            };

            m_Status = new Label(string.Empty)
            {
                style = { flexGrow = 1f, color = new Color(0.6f, 0.65f, 0.72f), whiteSpace = WhiteSpace.Normal }
            };
            footer.Add(m_Status);

            m_Prev = new Button(() => { if (m_Page > 0) { m_Page--; Reload(); } }) { text = "‹ Prev" };
            m_PageInfo = new Label(string.Empty) { style = { marginLeft = 6f, marginRight = 6f } };
            m_Next = new Button(() => { m_Page++; Reload(); }) { text = "Next ›" };
            footer.Add(m_Prev);
            footer.Add(m_PageInfo);
            footer.Add(m_Next);

            return footer;
        }

        void OnSearchChanged()
        {
            m_SearchDebounce?.Pause();
            m_SearchDebounce = rootVisualElement.schedule.Execute(ReloadFromFirstPage).StartingIn(300);
        }

        void ReloadFromFirstPage()
        {
            m_Page = 0;
            Reload();
        }

        async void Reload()
        {
            CancelInFlight();
            m_Cts = new CancellationTokenSource();
            var ct = m_Cts.Token;

            var settings = AssetCatalogSettings.GetOrCreate();
            m_Api = CatalogServices.CreateApi(settings);
            m_Thumbs = new ThumbnailService(
                CatalogServices.CreateHttpClient(settings), CatalogServices.CreateEndpoints(settings), m_Cache);

            m_SelectedCard = null;
            m_Details.ClearSelection();
            SetStatus("Loading…");

            var query = new SearchQuery
            {
                Text = m_Search.value,
                Type = m_TypeFilter.value,
                Page = m_Page,
                PageSize = settings.PageSize
            };

            var result = await m_Api.SearchAsync(query, ct);
            if (ct.IsCancellationRequested)
                return;

            m_Grid.Clear();

            if (!result.IsSuccess)
            {
                m_Total = 0;
                SetStatus($"✗ {ErrorText.Describe(result)}");
                UpdatePaging();
                return;
            }

            m_Total = result.Value.total;
            var items = result.Value.items ?? System.Array.Empty<CatalogEntryDto>();

            foreach (var item in items)
            {
                var card = new CatalogEntryCard(item, OnCardClicked);
                m_Grid.Add(card);
                LoadThumbnail(card, ct);
            }

            SetStatus(items.Length == 0 ? "No assets found." : $"Showing {items.Length} of {m_Total}.");
            UpdatePaging();
        }

        async void LoadThumbnail(CatalogEntryCard card, CancellationToken ct)
        {
            var key = $"{card.Entry.id}_{card.Entry.version}";
            var tex = await m_Thumbs.GetAsync(key, card.Entry.thumbnailUrl, ct);
            if (!ct.IsCancellationRequested && tex != null)
                card.SetThumbnail(tex);
        }

        void OnCardClicked(CatalogEntryDto entry)
        {
            m_SelectedCard?.SetSelected(false);
            m_SelectedCard = FindCard(entry);
            m_SelectedCard?.SetSelected(true);

            if (m_Cts != null)
                m_Details.Show(entry, m_Api, m_Thumbs, m_Cts.Token);
        }

        async void OnDownloadRequested(CatalogEntryDto entry)
        {
            m_DownloadCts?.Cancel();
            m_DownloadCts?.Dispose();
            m_DownloadCts = new CancellationTokenSource();
            var ct = m_DownloadCts.Token;

            m_Details.SetBusy(true);
            m_Details.SetDownloadStatus("Downloading…", false);

            try
            {
                var installer = CatalogServices.CreateInstaller(AssetCatalogSettings.GetOrCreate());
                var progress = new ActionProgress<float>(m_Details.ReportProgress);
                var result = await installer.InstallAsync(entry.id, progress, ct);

                if (!ct.IsCancellationRequested)
                    m_Details.SetDownloadStatus(result.Success ? $"✓ {result.Message}" : $"✗ {result.Message}", !result.Success);
            }
            catch (System.Exception e)
            {
                m_Details.SetDownloadStatus($"✗ {e.Message}", true);
            }
            finally
            {
                m_Details.SetBusy(false);
            }
        }

        CatalogEntryCard FindCard(CatalogEntryDto entry)
        {
            foreach (var child in m_Grid.Children())
            {
                if (child is CatalogEntryCard card && card.Entry == entry)
                    return card;
            }

            return null;
        }

        void UpdatePaging()
        {
            var settings = AssetCatalogSettings.GetOrCreate();
            var pageCount = Mathf.Max(1, Mathf.CeilToInt(m_Total / (float)Mathf.Max(1, settings.PageSize)));
            m_PageInfo.text = $"Page {m_Page + 1} / {pageCount}";
            m_Prev.SetEnabled(m_Page > 0);
            m_Next.SetEnabled(m_Page < pageCount - 1);
        }

        void SetStatus(string message)
        {
            if (m_Status != null)
                m_Status.text = message;
        }

        void CancelInFlight()
        {
            m_Cts?.Cancel();
            m_Cts?.Dispose();
            m_Cts = null;
        }
    }
}

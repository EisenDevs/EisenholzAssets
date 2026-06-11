using System;
using Eisenholz.AssetCatalog.Editor.Models;
using UnityEngine;
using UnityEngine.UIElements;

namespace Eisenholz.AssetCatalog.Editor.UI
{
    /// <summary>A single clickable tile in the thumbnail grid.</summary>
    public sealed class CatalogEntryCard : VisualElement
    {
        static readonly Color k_Border = new Color(0.18f, 0.19f, 0.22f);
        static readonly Color k_Selected = new Color(0.30f, 0.55f, 0.95f);
        static readonly Color k_Placeholder = new Color(0.12f, 0.13f, 0.15f);

        public CatalogEntryDto Entry { get; }

        readonly Image m_Image;

        public CatalogEntryCard(CatalogEntryDto entry, Action<CatalogEntryDto> onClick)
        {
            Entry = entry;

            style.width = 120f;
            style.marginRight = 8f;
            style.marginBottom = 8f;
            style.paddingTop = 6f;
            style.paddingBottom = 6f;
            style.paddingLeft = 6f;
            style.paddingRight = 6f;
            style.backgroundColor = new Color(0.16f, 0.17f, 0.19f);
            SetBorder(k_Border, 1f);
            style.borderTopLeftRadius = 6f;
            style.borderTopRightRadius = 6f;
            style.borderBottomLeftRadius = 6f;
            style.borderBottomRightRadius = 6f;

            m_Image = new Image
            {
                scaleMode = ScaleMode.ScaleToFit,
                style =
                {
                    height = 96f,
                    backgroundColor = k_Placeholder,
                    borderTopLeftRadius = 4f,
                    borderTopRightRadius = 4f,
                    borderBottomLeftRadius = 4f,
                    borderBottomRightRadius = 4f
                }
            };
            Add(m_Image);

            Add(new Label(entry.name)
            {
                tooltip = entry.name,
                style = { whiteSpace = WhiteSpace.Normal, fontSize = 11f, marginTop = 4f }
            });

            Add(new Label(entry.type)
            {
                style = { fontSize = 9f, color = new Color(0.55f, 0.6f, 0.68f) }
            });

            RegisterCallback<ClickEvent>(_ => onClick(entry));
        }

        public void SetThumbnail(Texture2D tex)
        {
            if (tex != null)
                m_Image.image = tex;
        }

        public void SetSelected(bool selected) => SetBorder(selected ? k_Selected : k_Border, selected ? 2f : 1f);

        void SetBorder(Color color, float width)
        {
            style.borderTopColor = color;
            style.borderBottomColor = color;
            style.borderLeftColor = color;
            style.borderRightColor = color;
            style.borderTopWidth = width;
            style.borderBottomWidth = width;
            style.borderLeftWidth = width;
            style.borderRightWidth = width;
        }
    }
}

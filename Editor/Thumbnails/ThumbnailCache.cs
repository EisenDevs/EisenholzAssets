using System;
using System.Collections.Generic;
using System.IO;
using Eisenholz.AssetCatalog.Editor.Utils;
using UnityEngine;

namespace Eisenholz.AssetCatalog.Editor.Thumbnails
{
    /// <summary>
    /// Two-tier thumbnail cache: an in-memory LRU of decoded textures plus a raw-bytes disk cache
    /// under <c>Library/AssetCatalog/Thumbnails</c> (machine-local, gitignored, never imported by the
    /// AssetDatabase). Owns the textures it decodes and destroys them on eviction / dispose.
    /// </summary>
    public sealed class ThumbnailCache : IDisposable
    {
        const int k_MemoryCap = 200;

        readonly string m_DiskDir;
        readonly Dictionary<string, Texture2D> m_Memory = new Dictionary<string, Texture2D>();
        readonly LinkedList<string> m_Lru = new LinkedList<string>();

        public ThumbnailCache()
        {
            m_DiskDir = Path.Combine("Library", "AssetCatalog", "Thumbnails");
            Directory.CreateDirectory(m_DiskDir);
        }

        public Texture2D GetFromMemory(string key)
        {
            if (!m_Memory.TryGetValue(key, out var tex) || tex == null)
                return null;

            m_Lru.Remove(key);
            m_Lru.AddFirst(key);
            return tex;
        }

        public void AddToMemory(string key, Texture2D tex)
        {
            if (tex == null)
                return;

            // If a texture is already stored under this key (e.g. two concurrent decodes raced the same
            // entry), destroy the previous instance before replacing it — otherwise it leaks.
            if (m_Memory.TryGetValue(key, out var existing))
            {
                m_Lru.Remove(key);
                if (existing != null && existing != tex)
                    UnityEngine.Object.DestroyImmediate(existing);
            }

            m_Memory[key] = tex;
            m_Lru.AddFirst(key);

            while (m_Memory.Count > k_MemoryCap && m_Lru.Last != null)
            {
                var evictKey = m_Lru.Last.Value;
                m_Lru.RemoveLast();
                if (m_Memory.TryGetValue(evictKey, out var evicted))
                {
                    m_Memory.Remove(evictKey);
                    if (evicted != null)
                        UnityEngine.Object.DestroyImmediate(evicted);
                }
            }
        }

        public bool TryReadDisk(string key, out byte[] bytes)
        {
            var path = DiskPath(key);
            if (File.Exists(path))
            {
                bytes = File.ReadAllBytes(path);
                return bytes.Length > 0;
            }

            bytes = null;
            return false;
        }

        public void WriteDisk(string key, byte[] bytes)
        {
            try
            {
                File.WriteAllBytes(DiskPath(key), bytes);
            }
            catch (Exception)
            {
                // Cache writes are best-effort; a failure just means we re-fetch next time.
            }
        }

        string DiskPath(string key) => Path.Combine(m_DiskDir, FormatUtils.SafeFileName(key) + ".thumb");

        public void Dispose()
        {
            foreach (var tex in m_Memory.Values)
            {
                if (tex != null)
                    UnityEngine.Object.DestroyImmediate(tex);
            }

            m_Memory.Clear();
            m_Lru.Clear();
        }
    }
}

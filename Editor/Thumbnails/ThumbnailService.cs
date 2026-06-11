using System.Threading;
using System.Threading.Tasks;
using Eisenholz.AssetCatalog.Editor.Api;
using Eisenholz.AssetCatalog.Editor.Http;
using UnityEngine;

namespace Eisenholz.AssetCatalog.Editor.Thumbnails
{
    /// <summary>
    /// Resolves and decodes thumbnails, going memory → disk → network. Decoding happens on the main
    /// thread (required for Texture2D). Returns null when no thumbnail is available.
    /// </summary>
    public sealed class ThumbnailService
    {
        readonly IHttpClient m_Http;
        readonly CatalogEndpoints m_Endpoints;
        readonly ThumbnailCache m_Cache;

        public ThumbnailService(IHttpClient http, CatalogEndpoints endpoints, ThumbnailCache cache)
        {
            m_Http = http;
            m_Endpoints = endpoints;
            m_Cache = cache;
        }

        public async Task<Texture2D> GetAsync(string key, string thumbnailUrl, CancellationToken ct)
        {
            var fromMemory = m_Cache.GetFromMemory(key);
            if (fromMemory != null)
                return fromMemory;

            if (m_Cache.TryReadDisk(key, out var diskBytes))
            {
                var diskTex = Decode(diskBytes);
                if (diskTex != null)
                    m_Cache.AddToMemory(key, diskTex);
                return diskTex;
            }

            var url = m_Endpoints.Resolve(thumbnailUrl);
            if (string.IsNullOrEmpty(url))
                return null;

            var result = await m_Http.GetBytesAsync(url, ct);
            if (!result.IsSuccess || result.Value == null || result.Value.Length == 0)
                return null;

            m_Cache.WriteDisk(key, result.Value);
            var tex = Decode(result.Value);
            if (tex != null)
                m_Cache.AddToMemory(key, tex);
            return tex;
        }

        static Texture2D Decode(byte[] bytes)
        {
            var tex = new Texture2D(2, 2);
            if (tex.LoadImage(bytes))
                return tex;

            Object.DestroyImmediate(tex);
            return null;
        }
    }
}

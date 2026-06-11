using System;

namespace Eisenholz.AssetCatalog.Editor.Models
{
    /// <summary>
    /// Tells the client which importer to use and where the bytes live. Returned by
    /// GET /assets/{id}/manifest.
    /// </summary>
    [Serializable]
    public sealed class AssetManifestDto
    {
        public string id;
        public string version;
        public string format;          // "unitypackage" | "zip-meta" | "raw"
        public string downloadUrl;
        public string sha256;
        public long sizeBytes;
        public string suggestedPath;   // e.g. "Assets/Eisenholz/Trees"
        public AssetFileDto[] files;   // populated for raw / zip-meta; informational for unitypackage
    }

    /// <summary>One file inside a multi-file asset (raw / zip-meta formats).</summary>
    [Serializable]
    public sealed class AssetFileDto
    {
        public string relativePath;
        public long sizeBytes;
        public string sha256;
    }
}

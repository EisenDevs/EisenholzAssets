using System;

namespace Eisenholz.AssetCatalog.Editor.Models
{
    // NOTE: field names are camelCase ON PURPOSE — they must match the JSON keys exactly because
    // JsonUtility is case-sensitive and has no name-mapping attributes. UI code should map these
    // wire DTOs onto its own view models rather than binding to them directly.

    /// <summary>One row in a catalog listing / search result.</summary>
    [Serializable]
    public sealed class CatalogEntryDto
    {
        public string id;
        public string name;
        public string type;        // model | texture | script | audio | prefab | material | shader | any
        public string version;
        public long sizeBytes;
        public string thumbnailUrl;
        public string[] tags;
        public string updatedUtc;
    }

    /// <summary>Wrapper for a list response. JsonUtility cannot parse a top-level array.</summary>
    [Serializable]
    public sealed class CatalogListResponseDto
    {
        public CatalogEntryDto[] items;
        public int total;
        public int page;
        public int pageSize;
    }

    /// <summary>Full detail for a single asset.</summary>
    [Serializable]
    public sealed class AssetDetailDto
    {
        public string id;
        public string name;
        public string type;
        public string version;
        public string description;
        public long sizeBytes;
        public string thumbnailUrl;
        public string[] tags;
        public KeyValueDto[] metadata;   // array of {key,value}, NOT a map (JsonUtility-safe)
        public string updatedUtc;
    }

    /// <summary>A single metadata pair. Used instead of a dictionary.</summary>
    [Serializable]
    public sealed class KeyValueDto
    {
        public string key;
        public string value;
    }

    /// <summary>Response of GET /health.</summary>
    [Serializable]
    public sealed class HealthDto
    {
        public string status;
        public string version;
    }
}

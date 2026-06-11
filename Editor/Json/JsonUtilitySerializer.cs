using UnityEngine;

namespace Eisenholz.AssetCatalog.Editor.Json
{
    /// <summary>
    /// <see cref="JsonUtility"/>-backed serializer. Constraints driven into the API contract:
    /// no top-level arrays (lists must be wrapped in an object), no dictionaries (use key/value
    /// arrays). DTO field names must match the JSON keys exactly (camelCase) — JsonUtility is
    /// case-sensitive and has no name-mapping attributes.
    /// </summary>
    public sealed class JsonUtilitySerializer : IJsonSerializer
    {
        public string Serialize<T>(T value) => JsonUtility.ToJson(value);

        public T Deserialize<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
                return default;

            // JsonUtility throws on malformed JSON; callers treat a null/empty result as a parse failure.
            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch (System.Exception)
            {
                return default;
            }
        }
    }
}

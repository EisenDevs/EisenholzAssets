namespace Eisenholz.AssetCatalog.Editor.Json
{
    /// <summary>
    /// JSON seam. Default implementation is <see cref="JsonUtilitySerializer"/>. If the contract ever
    /// needs maps/dictionaries or polymorphism, add the official Newtonsoft package and provide a
    /// Newtonsoft-backed implementation — no call sites change.
    /// </summary>
    public interface IJsonSerializer
    {
        string Serialize<T>(T value);
        T Deserialize<T>(string json);
    }
}

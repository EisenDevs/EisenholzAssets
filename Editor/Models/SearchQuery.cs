namespace Eisenholz.AssetCatalog.Editor.Models
{
    /// <summary>Parameters for a catalog search/list request. Not serialized — used to build the URL.</summary>
    public sealed class SearchQuery
    {
        public string Text = "";
        public string Type = "any";   // model | texture | script | any
        public int Page = 0;
        public int PageSize = 50;
    }
}

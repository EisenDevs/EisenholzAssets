using System.Collections.Generic;

namespace Eisenholz.AssetCatalog.Editor.Import
{
    /// <summary>Outcome of an extraction or import. Never thrown — returned as data.</summary>
    public sealed class ImportResult
    {
        public bool Success { get; }
        public string Message { get; }

        /// <summary>Project-relative (or absolute, for intermediate extractor results) paths that were written.</summary>
        public IReadOnlyList<string> ImportedPaths { get; }

        ImportResult(bool success, string message, IReadOnlyList<string> importedPaths)
        {
            Success = success;
            Message = message;
            ImportedPaths = importedPaths;
        }

        public static ImportResult Ok(IReadOnlyList<string> importedPaths, string message) =>
            new ImportResult(true, message, importedPaths);

        public static ImportResult Fail(string message) =>
            new ImportResult(false, message, System.Array.Empty<string>());
    }
}

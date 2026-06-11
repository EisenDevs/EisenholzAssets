using System;
using System.IO;

namespace Eisenholz.AssetCatalog.Editor.Utils
{
    /// <summary>Path helpers for keeping writes inside the project and inside a target directory.</summary>
    public static class ProjectPaths
    {
        /// <summary>Unity's working directory is the project root.</summary>
        public static string ProjectRoot => Directory.GetCurrentDirectory();

        public static string AssetsRoot => Path.GetFullPath("Assets");

        /// <summary>True if <paramref name="candidateFull"/> is the target dir or lives beneath it.</summary>
        public static bool IsContained(string targetFullDir, string candidateFull)
        {
            if (string.Equals(candidateFull, targetFullDir, StringComparison.Ordinal))
                return true;

            var prefix = targetFullDir.EndsWith(Path.DirectorySeparatorChar.ToString())
                ? targetFullDir
                : targetFullDir + Path.DirectorySeparatorChar;
            return candidateFull.StartsWith(prefix, StringComparison.Ordinal);
        }

        /// <summary>True if the given path (relative or absolute) resolves to somewhere under Assets/.</summary>
        public static bool IsUnderAssets(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;
            return IsContained(AssetsRoot, Path.GetFullPath(path));
        }

        public static string ToProjectRelative(string absolute) =>
            Path.GetRelativePath(ProjectRoot, absolute).Replace('\\', '/');
    }
}

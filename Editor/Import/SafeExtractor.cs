using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Eisenholz.AssetCatalog.Editor.Utils;

namespace Eisenholz.AssetCatalog.Editor.Import
{
    /// <summary>
    /// Extracts a zip archive into a target directory with hard safety guards:
    /// <list type="bullet">
    /// <item>Zip-slip: every entry's resolved path must stay inside the target directory.</item>
    /// <item>Rooted/absolute entry names are rejected.</item>
    /// <item>Per-entry and total uncompressed-size caps guard against zip bombs.</item>
    /// </list>
    /// All entries are validated BEFORE any file is written, so an unsafe archive writes nothing.
    /// </summary>
    public static class SafeExtractor
    {
        const long k_MaxEntryBytes = 1L << 30;   // 1 GB per entry
        const long k_MaxTotalBytes = 4L << 30;   // 4 GB total

        public static ImportResult ExtractZip(string zipPath, string targetDirAbsolute, bool overwrite)
        {
            var targetFull = Path.GetFullPath(targetDirAbsolute);

            try
            {
                using var fileStream = File.OpenRead(zipPath);
                using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read);

                // Pass 1 — validate everything before touching the disk.
                long totalBytes = 0;
                foreach (var entry in archive.Entries)
                {
                    if (string.IsNullOrEmpty(entry.FullName))
                        continue;

                    if (!TryResolveSafe(entry.FullName, targetFull, out _))
                        return ImportResult.Fail($"Rejected unsafe zip entry (path traversal): '{entry.FullName}'.");

                    if (entry.Length > k_MaxEntryBytes)
                        return ImportResult.Fail($"Zip entry too large: '{entry.FullName}'.");

                    totalBytes += entry.Length;
                    if (totalBytes > k_MaxTotalBytes)
                        return ImportResult.Fail("Zip archive exceeds the maximum allowed total size.");
                }

                // Pass 2 — extract.
                var written = new List<string>();
                foreach (var entry in archive.Entries)
                {
                    if (string.IsNullOrEmpty(entry.FullName))
                        continue;

                    TryResolveSafe(entry.FullName, targetFull, out var dest);

                    var isDirectory = entry.FullName.EndsWith("/") || entry.FullName.EndsWith("\\");
                    if (isDirectory)
                    {
                        Directory.CreateDirectory(dest);
                        continue;
                    }

                    Directory.CreateDirectory(Path.GetDirectoryName(dest));
                    if (File.Exists(dest) && !overwrite)
                        continue;

                    using (var entryStream = entry.Open())
                    using (var outStream = File.Create(dest))
                        entryStream.CopyTo(outStream);

                    written.Add(dest);
                }

                return ImportResult.Ok(written, $"Extracted {written.Count} file(s).");
            }
            catch (InvalidDataException)
            {
                return ImportResult.Fail("Downloaded file is not a valid zip archive.");
            }
            catch (Exception e)
            {
                return ImportResult.Fail($"Extraction failed: {e.Message}");
            }
        }

        /// <summary>Resolves an entry name under the target dir, returning false if it escapes or is rooted.</summary>
        static bool TryResolveSafe(string entryName, string targetFull, out string dest)
        {
            dest = null;
            var normalized = entryName.Replace('\\', '/');
            if (Path.IsPathRooted(normalized))
                return false;

            var combined = Path.GetFullPath(Path.Combine(targetFull, normalized));
            if (!ProjectPaths.IsContained(targetFull, combined))
                return false;

            dest = combined;
            return true;
        }
    }
}

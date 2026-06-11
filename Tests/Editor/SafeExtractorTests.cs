using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Eisenholz.AssetCatalog.Editor.Import;
using NUnit.Framework;

namespace Eisenholz.AssetCatalog.Tests
{
    public sealed class SafeExtractorTests
    {
        string m_Root;
        string m_Target;

        [SetUp]
        public void SetUp()
        {
            m_Root = Path.Combine(Path.GetTempPath(), "ehz_extract_" + Guid.NewGuid().ToString("N"));
            m_Target = Path.Combine(m_Root, "target");
            Directory.CreateDirectory(m_Target);
        }

        [TearDown]
        public void TearDown()
        {
            try { Directory.Delete(m_Root, recursive: true); }
            catch { /* best effort */ }
        }

        static void WriteZip(string zipPath, params (string name, string content)[] entries)
        {
            using var fs = File.Create(zipPath);
            using var archive = new ZipArchive(fs, ZipArchiveMode.Create);
            foreach (var (name, content) in entries)
            {
                var entry = archive.CreateEntry(name);
                using var stream = entry.Open();
                var bytes = Encoding.UTF8.GetBytes(content);
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        [Test]
        public void ExtractZip_WritesSafeEntries()
        {
            var zip = Path.Combine(m_Root, "safe.zip");
            WriteZip(zip, ("Trees/Oak.txt", "model"), ("Trees/Oak.txt.meta", "guid: 123"));

            var result = SafeExtractor.ExtractZip(zip, m_Target, overwrite: true);

            Assert.IsTrue(result.Success, result.Message);
            Assert.IsTrue(File.Exists(Path.Combine(m_Target, "Trees", "Oak.txt")));
            Assert.IsTrue(File.Exists(Path.Combine(m_Target, "Trees", "Oak.txt.meta")));
        }

        [Test]
        public void ExtractZip_RejectsPathTraversal_AndWritesNothing()
        {
            var zip = Path.Combine(m_Root, "evil.zip");
            WriteZip(zip, ("Trees/Ok.txt", "ok"), ("../escaped.txt", "pwned"));

            var result = SafeExtractor.ExtractZip(zip, m_Target, overwrite: true);

            Assert.IsFalse(result.Success);
            // Pre-validation must abort before any file is written.
            Assert.IsFalse(File.Exists(Path.Combine(m_Root, "escaped.txt")), "traversal file escaped the target");
            Assert.IsFalse(File.Exists(Path.Combine(m_Target, "Trees", "Ok.txt")), "nothing should be written when the archive is unsafe");
        }

        [Test]
        public void ExtractZip_InvalidArchive_FailsGracefully()
        {
            var notZip = Path.Combine(m_Root, "notzip.zip");
            File.WriteAllText(notZip, "this is not a zip");

            var result = SafeExtractor.ExtractZip(notZip, m_Target, overwrite: true);

            Assert.IsFalse(result.Success);
        }
    }
}

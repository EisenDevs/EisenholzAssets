using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Eisenholz.AssetCatalog.Editor.Utils
{
    public static class Sha256
    {
        /// <summary>Streams the file through SHA-256 and returns the lowercase hex digest.</summary>
        public static string OfFile(string path)
        {
            using var stream = File.OpenRead(path);
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(stream);

            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}

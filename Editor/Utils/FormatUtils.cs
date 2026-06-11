using System;

namespace Eisenholz.AssetCatalog.Editor.Utils
{
    public static class FormatUtils
    {
        static readonly string[] k_Units = { "B", "KB", "MB", "GB", "TB" };

        /// <summary>Human-readable byte size, e.g. 5242880 → "5.0 MB".</summary>
        public static string Bytes(long bytes)
        {
            if (bytes <= 0)
                return "0 B";

            double size = bytes;
            var unit = 0;
            while (size >= 1024d && unit < k_Units.Length - 1)
            {
                size /= 1024d;
                unit++;
            }

            return unit == 0 ? $"{bytes} B" : $"{size:0.0} {k_Units[unit]}";
        }

        /// <summary>Replaces characters that are invalid in a file name with '_'.</summary>
        public static string SafeFileName(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "_";

            var chars = value.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                var c = chars[i];
                var ok = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') ||
                         c == '-' || c == '_' || c == '.';
                if (!ok)
                    chars[i] = '_';
            }

            return new string(chars);
        }
    }
}

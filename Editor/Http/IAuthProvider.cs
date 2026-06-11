namespace Eisenholz.AssetCatalog.Editor.Http
{
    /// <summary>
    /// Supplies an authentication header for catalog requests. Today the catalog runs on a
    /// trusted network and <see cref="NullAuthProvider"/> is used. When auth is added later,
    /// implement this (e.g. a token provider) and inject it into the HTTP client — no call site
    /// changes are required.
    /// </summary>
    public interface IAuthProvider
    {
        /// <summary>Returns true and the header name/value when auth should be attached.</summary>
        bool TryGetAuthHeader(out string name, out string value);
    }

    /// <summary>No-op auth provider for the trusted-network configuration.</summary>
    public sealed class NullAuthProvider : IAuthProvider
    {
        public bool TryGetAuthHeader(out string name, out string value)
        {
            name = null;
            value = null;
            return false;
        }
    }
}

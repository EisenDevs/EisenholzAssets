using System;

namespace Eisenholz.AssetCatalog.Editor.Utils
{
    /// <summary>
    /// Synchronous <see cref="IProgress{T}"/>. Unlike <see cref="System.Progress{T}"/> it does not post
    /// to a SynchronizationContext — our awaits already resume on the main thread, so reporting inline
    /// keeps the UI update on that same thread immediately.
    /// </summary>
    public sealed class ActionProgress<T> : IProgress<T>
    {
        readonly Action<T> m_OnReport;

        public ActionProgress(Action<T> onReport) => m_OnReport = onReport;

        public void Report(T value) => m_OnReport?.Invoke(value);
    }
}

using System;
using System.Runtime.CompilerServices;
using UnityEngine.Networking;

namespace Eisenholz.AssetCatalog.Editor.Http
{
    /// <summary>
    /// Lets us <c>await</c> a <see cref="UnityWebRequestAsyncOperation"/> directly. The continuation
    /// runs on the main thread (the operation's <c>completed</c> event fires there), which is required
    /// because UnityWebRequest and AssetDatabase are main-thread only.
    /// </summary>
    public readonly struct UnityWebRequestAwaiter : INotifyCompletion
    {
        readonly UnityWebRequestAsyncOperation m_Operation;

        public UnityWebRequestAwaiter(UnityWebRequestAsyncOperation operation) => m_Operation = operation;

        public bool IsCompleted => m_Operation.isDone;

        public void GetResult() { }

        public void OnCompleted(Action continuation)
        {
            if (m_Operation.isDone)
                continuation();
            else
                m_Operation.completed += _ => continuation();
        }
    }

    public static class EditorAwaiters
    {
        public static UnityWebRequestAwaiter GetAwaiter(this UnityWebRequestAsyncOperation operation) =>
            new UnityWebRequestAwaiter(operation);
    }
}

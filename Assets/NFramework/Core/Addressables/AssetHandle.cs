#if ADDRESSABLES
using System;

namespace NFramework
{
    public sealed class AssetHandle<T> : IDisposable
    {
        private Action _onDispose;

        public T Result { get; }
        public string Key { get; }
        public bool IsDisposed { get; private set; }

        internal AssetHandle(string key, T result, Action onDispose)
        {
            Key = key;
            Result = result;
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            if (IsDisposed)
            {
                NLogger.LogError($"[AssetHandle] Handle for key '{Key}' has already been disposed.");
                return;
            }

            IsDisposed = true;
            _onDispose?.Invoke();
            _onDispose = null;
        }
    }
}
#endif

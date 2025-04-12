using Cysharp.Threading.Tasks;
using System;

namespace NFramework
{
    public abstract class BaseAddressableLoader
    {
        public string Key { get; protected set; }
        public EAddressableOperationStatus Status { get; protected set; }
        
        protected BaseAddressableLoader(string key)
        {
            Key = key;
        }

        public abstract UniTask Load();
        public abstract void Release();
    }
}
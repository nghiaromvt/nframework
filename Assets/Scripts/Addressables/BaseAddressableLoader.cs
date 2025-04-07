using Cysharp.Threading.Tasks;
using System;

namespace NFramework
{
    public enum EAddressableLoaderStatus { None, Loading, Success, Failed, Error, Released }
    
    public abstract class BaseAddressableLoader
    {
        public string Key { get; protected set; }
        public EAddressableLoaderStatus Status { get; protected set; }
        
        protected BaseAddressableLoader(string key)
        {
            Key = key;
        }

        public abstract UniTask Load();
        public abstract void Release();
    }
}
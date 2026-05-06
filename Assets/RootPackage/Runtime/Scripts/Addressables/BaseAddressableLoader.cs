#if ADDRESSABLES
using Cysharp.Threading.Tasks;

namespace NFramework
{
    public abstract class BaseAddressableLoader : AddressableOperator
    {
        protected BaseAddressableLoader(string key)
        {
            Key = key;
        }

        public abstract UniTask Load();
        public abstract void Release();
    }
}
#endif
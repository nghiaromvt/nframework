using System;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace NFramework
{
    public class AddressableAssetLoader<T> : BaseAddressableLoader where T : Object
    {
        private AsyncOperationHandle<T> _handle;
        
        public AddressableAssetLoader(string key) : base(key)
        {
        }

        public override async UniTask Load()
        {
            if (Status != EAddressableLoaderStatus.None) return;

            try
            {
                Status = EAddressableLoaderStatus.Loading;
                _handle = Addressables.LoadAssetAsync<T>(Key);
                await _handle;
                
                if (_handle.Status == AsyncOperationStatus.Succeeded)
                {
                    AddressablesManager.Log($"Succeed to load asset with address:{Key}");
                    Status = EAddressableLoaderStatus.Success;
                }
                else
                {
                    AddressablesManager.LogError($"Failed to load asset with address:{Key}");
                    Status = EAddressableLoaderStatus.Failed;
                    Release();
                }
            }
            catch (Exception e)
            {
                AddressablesManager.LogError(e.Message);
                Status = EAddressableLoaderStatus.Error;
                Release();
            }
        }

        public override void Release()
        {
            if (Status == EAddressableLoaderStatus.Released) return;
            Status = EAddressableLoaderStatus.Released;
            if (_handle.IsValid()) Addressables.Release(_handle);
        }

        public T GetResult() => Status != EAddressableLoaderStatus.Success ? null : _handle.Result;
    }
}
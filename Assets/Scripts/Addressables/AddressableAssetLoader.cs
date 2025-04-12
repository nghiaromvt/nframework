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
            if (Status != EAddressableOperationStatus.None) return;

            try
            {
                Status = EAddressableOperationStatus.Operating;
                _handle = Addressables.LoadAssetAsync<T>(Key);
                await _handle;
                
                if (_handle.Status == AsyncOperationStatus.Succeeded)
                {
                    AddressablesManager.Log($"Succeed to load asset with key: {Key}");
                    Status = EAddressableOperationStatus.Success;
                }
                else
                {
                    AddressablesManager.LogError($"Failed to load asset with key: {Key}");
                    Status = EAddressableOperationStatus.Failed;
                    Release();
                }
            }
            catch (Exception e)
            {
                AddressablesManager.LogError(e.Message);
                Status = EAddressableOperationStatus.Error;
                Release();
            }
        }

        public override void Release()
        {
            if (Status == EAddressableOperationStatus.Released) return;
            Status = EAddressableOperationStatus.Released;
            if (_handle.IsValid()) Addressables.Release(_handle);
        }

        public T GetResult() => Status != EAddressableOperationStatus.Success ? null : _handle.Result;
    }
}
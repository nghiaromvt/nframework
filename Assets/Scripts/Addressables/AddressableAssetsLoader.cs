using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace NFramework
{
    public class AddressableAssetsLoader<T> : BaseAddressableLoader where T : Object
    {
        private AsyncOperationHandle<IList<T>> _handle;
        
        public AddressableAssetsLoader(string key) : base(key)
        {
        }

        public override async UniTask Load()
        {
            if (Status != EAddressableLoaderStatus.None) return;

            try
            {
                Status = EAddressableLoaderStatus.Loading;
                _handle = Addressables.LoadAssetsAsync<T>(Key, null, true);
                await _handle;
                
                if (_handle.Status == AsyncOperationStatus.Succeeded)
                {
                    AddressablesManager.Log($"Succeed to load assets with label:{Key}");
                    Status = EAddressableLoaderStatus.Success;
                }
                else
                {
                    AddressablesManager.LogError($"Failed to load assets with label:{Key}");
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

        public List<T> GetResult() => Status != EAddressableLoaderStatus.Success ? null : _handle.Result.ToList();
    }
}
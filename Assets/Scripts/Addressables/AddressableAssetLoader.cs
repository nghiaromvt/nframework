#if ADDRESSABLES
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
            if (Status != AddressableOperationStatus.None) return;

            try
            {
                Status = AddressableOperationStatus.Operating;
                _handle = Addressables.LoadAssetAsync<T>(Key);
                
                var progressPercent = 0f;

                while (Status == AddressableOperationStatus.Operating && _handle.Status == AsyncOperationStatus.None)
                {
                    var downloadStatus = _handle.GetDownloadStatus();
                    if (downloadStatus.Percent > progressPercent * 1.1) // Report at most every 10% or so
                    {
                        progressPercent = downloadStatus.Percent; // More accurate %
                        OnProgress?.Invoke(downloadStatus.DownloadedBytes, downloadStatus.TotalBytes, progressPercent);
                    }

                    await UniTask.Yield();
                }
                
                if (_handle.Status == AsyncOperationStatus.Succeeded)
                {
                    AddressablesManager.Log($"Succeed to load asset with key: {Key}");
                    Status = AddressableOperationStatus.Success;
                }
                else
                {
                    AddressablesManager.LogError($"Failed to load asset with key: {Key}");
                    Status = AddressableOperationStatus.Failed;
                    Release();
                }
            }
            catch (Exception e)
            {
                AddressablesManager.LogError(e.Message);
                Status = AddressableOperationStatus.Error;
                Release();
            }
        }

        public override void Release()
        {
            if (Status == AddressableOperationStatus.Released) return;
            Status = AddressableOperationStatus.Released;
            if (_handle.IsValid()) Addressables.Release(_handle);
        }

        public T GetResult() => Status != AddressableOperationStatus.Success ? null : _handle.Result;
    }
}
#endif
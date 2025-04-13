using System;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace NFramework
{
    public class AddressableAssetDownloader : AddressableOperator
    {
        private AsyncOperationHandle _handle;

        public AddressableAssetDownloader(string key)
        {
            Key = key;
        }

        public async UniTask<bool> Download()
        {
            if (Status != EAddressableOperationStatus.None) return false;
            
            try
            {
                Status = EAddressableOperationStatus.Operating;
                _handle = Addressables.DownloadDependenciesAsync(Key, false);
                
                var progressPercent = 0f;

                while (Status == EAddressableOperationStatus.Operating && _handle.Status == AsyncOperationStatus.None)
                {
                    var downloadStatus = _handle.GetDownloadStatus();
                    if (downloadStatus.Percent > progressPercent * 1.1) // Report at most every 10% or so
                    {
                        progressPercent = downloadStatus.Percent; // More accurate %
                        OnProgress?.Invoke(downloadStatus.DownloadedBytes, downloadStatus.TotalBytes, progressPercent);
                    }

                    await UniTask.Yield();
                }
                
                if (Status != EAddressableOperationStatus.Operating)
                    return false;
                
                if (_handle.Status == AsyncOperationStatus.Succeeded)
                {
                    AddressablesManager.Log($"Succeed to download asset with key: {Key}");
                    Status = EAddressableOperationStatus.Success;
                    return true;
                }
                else
                {
                    AddressablesManager.LogError($"Failed to load asset with address: {Key}");
                    Status = EAddressableOperationStatus.Failed;
                    Release();
                    return false;
                }
            }
            catch (Exception e)
            {
                AddressablesManager.LogError(e.Message);
                Status = EAddressableOperationStatus.Error;
                Release();
                return false;
            }
        }

        public void Release()
        {
            if (Status == EAddressableOperationStatus.Released) return;
            Status = EAddressableOperationStatus.Released;
            if (_handle.IsValid()) Addressables.Release(_handle);
        }
    }
}
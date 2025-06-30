#if ADDRESSABLES
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
            if (Status != AddressableOperationStatus.None) return false;
            
            try
            {
                Status = AddressableOperationStatus.Operating;
                _handle = Addressables.DownloadDependenciesAsync(Key, false);
                
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
                
                if (Status != AddressableOperationStatus.Operating)
                    return false;
                
                if (_handle.Status == AsyncOperationStatus.Succeeded)
                {
                    AddressablesManager.Log($"Succeed to download asset with key: {Key}");
                    Status = AddressableOperationStatus.Success;
                    return true;
                }
                else
                {
                    AddressablesManager.LogError($"Failed to load asset with address: {Key}");
                    Status = AddressableOperationStatus.Failed;
                    Release();
                    return false;
                }
            }
            catch (Exception e)
            {
                AddressablesManager.LogError(e.Message);
                Status = AddressableOperationStatus.Error;
                Release();
                return false;
            }
        }

        public void Release()
        {
            if (Status == AddressableOperationStatus.Released) return;
            Status = AddressableOperationStatus.Released;
            if (_handle.IsValid()) Addressables.Release(_handle);
        }
    }
}
#endif
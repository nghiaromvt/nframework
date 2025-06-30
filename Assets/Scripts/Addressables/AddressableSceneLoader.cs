#if ADDRESSABLES
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace NFramework
{
    public class AddressableSceneLoader : BaseAddressableLoader
    {
        private readonly LoadSceneMode _loadSceneMode;
        private readonly bool _activateOnLoad;
        private readonly bool _setActiveScene;
        private AsyncOperationHandle<SceneInstance> _handle;
        
        public AddressableSceneLoader(string key, LoadSceneMode loadSceneMode = LoadSceneMode.Single, 
            bool setActiveScene = true, bool activateOnLoad = true) : base(key)
        {
            _loadSceneMode = loadSceneMode;
            _activateOnLoad = activateOnLoad;
            _setActiveScene = setActiveScene;
        }

        public override async UniTask Load()
        {
            if (Status != AddressableOperationStatus.None) return;
            
            try
            {
                Status = AddressableOperationStatus.Operating;
                _handle = Addressables.LoadSceneAsync(Key, _loadSceneMode, _activateOnLoad);
                
                var progressPercent = 0f;

                while (Status == AddressableOperationStatus.Operating && _handle.Status == AsyncOperationStatus.None)
                {
                    var downloadStatus = _handle.GetDownloadStatus();
                    if (downloadStatus.Percent > progressPercent * 1.1) // Report at most every 10% or so
                    {
                        progressPercent = downloadStatus.Percent; // More accurate %
                        OnProgress?.Invoke(downloadStatus.DownloadedBytes, downloadStatus.TotalBytes, progressPercent);
                    }

                    await UniTask.NextFrame();
                }
                
                if (_handle.Status == AsyncOperationStatus.Succeeded)
                {
                    AddressablesManager.Log($"Succeed to load scene with key: {Key}");
                    Status = AddressableOperationStatus.Success;
                    if (_activateOnLoad && _setActiveScene) SceneManager.SetActiveScene(_handle.Result.Scene);
                }
                else
                {
                    AddressablesManager.LogError($"Failed to load scene with key: {Key}");
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
            Unload().Forget();
        }
        
        public SceneInstance GetResult() => Status != AddressableOperationStatus.Success ? default : _handle.Result;

        public async UniTask Unload()
        {
            if (_handle.IsValid() && _handle.Result.Scene.IsValid() && _handle.Result.Scene.isLoaded)
            {
                var unloadHandle =  Addressables.UnloadSceneAsync(_handle, false);
                await unloadHandle;
                Addressables.Release(unloadHandle);
            }
        }
    }
}
#endif

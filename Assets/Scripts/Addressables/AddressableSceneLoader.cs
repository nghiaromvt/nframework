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
            if (Status != EAddressableOperationStatus.None) return;
            
            try
            {
                Status = EAddressableOperationStatus.Operating;
                _handle = Addressables.LoadSceneAsync(Key, _loadSceneMode, _activateOnLoad);
                await _handle;
                
                if (_handle.Status == AsyncOperationStatus.Succeeded)
                {
                    AddressablesManager.Log($"Succeed to load scene with key: {Key}");
                    Status = EAddressableOperationStatus.Success;
                    if (_activateOnLoad && _setActiveScene) SceneManager.SetActiveScene(_handle.Result.Scene);
                }
                else
                {
                    AddressablesManager.LogError($"Failed to load scene with key: {Key}");
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
            Unload().Forget();
        }
        
        public SceneInstance GetResult() => Status != EAddressableOperationStatus.Success ? default : _handle.Result;

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
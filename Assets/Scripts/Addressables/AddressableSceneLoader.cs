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
            if (Status != EAddressableLoaderStatus.None) return;
            
            try
            {
                Status = EAddressableLoaderStatus.Loading;
                _handle = Addressables.LoadSceneAsync(Key, _loadSceneMode, _activateOnLoad);
                await _handle;
                
                if (_handle.Status == AsyncOperationStatus.Succeeded)
                {
                    AddressablesManager.Log($"Succeed to load scene with address:{Key}");
                    Status = EAddressableLoaderStatus.Success;
                    if (_activateOnLoad && _setActiveScene) SceneManager.SetActiveScene(_handle.Result.Scene);
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
            Unload().Forget();
        }
        
        public SceneInstance GetResult() => Status != EAddressableLoaderStatus.Success ? default : _handle.Result;

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
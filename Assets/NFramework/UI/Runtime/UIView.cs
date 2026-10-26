using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

[assembly: InternalsVisibleTo("NFramework.UI.Editor")]

namespace NFramework.UI
{
    public enum EViewStatus
    {
        Closed,
        Opening,
        Open,
        Closing
    }

    public class UIView : MonoBehaviour
    {
        #region Fields

        [SerializeField] private UILayer _uiLayer;
        [SerializeField] private bool _pauseGameStatus;

        [HideLabel, ReadOnly, ShowInInspector, ShowIf(nameof(HasError)), GUIColor(1, 0.3f, 0.3f)]
        private string _errorMessage;

        private CanvasGroup _canvasGroup;
        private bool _initialized;
        private IOpenViewAnimation[] _openViewAnimations;
        private ICloseViewAnimation[] _closeViewAnimations;
        private CancellationTokenSource _openCts;
        private UniTask? _closingTask;
        private bool _isInteractBlocked;

        #endregion

        #region Properties

        private bool HasError => !string.IsNullOrEmpty(_errorMessage);

        [field: SerializeField, ReadOnly] public string DefineKeyConstName { get; internal set; }
        [field: SerializeField, ReadOnly] public string Key { get; internal set; }

        public UILayer UILayer => _uiLayer;
        public bool PauseGameStatus { get; protected set; }
        public string ID { get; private set; }
        public bool IsFromResources { get; private set; }
        public EViewStatus ViewStatus { get; private set; }

        public CanvasGroup CanvasGroup
        {
            get
            {
                if (!_canvasGroup)
                    _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();

                return _canvasGroup;
            }
        }

        #endregion

        #region Unity Lifecycle Functions

        private void OnDisable()
        {
            CancelOpenAnimation();
            UnblockInteract();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            RefreshKey();
            ValidateKey();
        }

        private void OnValidate()
        {
            if (Application.isPlaying) return;
            RefreshKey();
            ValidateKey();
        }
#endif

        #endregion

        #region Custom Methods - Public

        public void Initialize(string id, bool isFromResources = false)
        {
            if (_initialized) return;
            _initialized = true;
            ID = id;
            IsFromResources = isFromResources;
            _openViewAnimations = GetComponents<IOpenViewAnimation>();
            _closeViewAnimations = GetComponents<ICloseViewAnimation>();
            OnInitialized();
        }

        public void OnOpen(UIInputData inputData = null)
        {
            if (!_initialized)
                Initialize(gameObject.name);

            _closingTask = null;
            ViewStatus = EViewStatus.Opening;

            var pauseStatus = inputData?.PauseStatus ?? UIInputData.EPauseGameStatus.UseDefault;
            PauseGameStatus = pauseStatus switch
            {
                UIInputData.EPauseGameStatus.UseDefault => _pauseGameStatus,
                UIInputData.EPauseGameStatus.Pause => true,
                UIInputData.EPauseGameStatus.NoPause => false,
                _ => PauseGameStatus
            };

            if (PauseGameStatus)
                PauseGameHandler.Pause(this);

            OnViewOpened(inputData);

            ExecuteOpenAsync().Forget();
        }

        public UniTask OnClose(bool ignoreAnim = false, CancellationToken cancellationToken = default)
        {
            if (ViewStatus == EViewStatus.Closed)
                return UniTask.CompletedTask;

            if (ViewStatus == EViewStatus.Closing && _closingTask.HasValue)
                return _closingTask.Value;

            if (ViewStatus == EViewStatus.Opening)
                CancelOpenAnimation();

            _closingTask = ExecuteCloseAsync(ignoreAnim, cancellationToken);
            return _closingTask.Value;
        }

        public UniTask CloseSelf(bool destroy = false, bool ignoreAnim = false, CancellationToken cancellationToken = default)
        {
            return UIManager.Close(this, destroy, ignoreAnim, cancellationToken);
        }

        protected virtual void OnInitialized()
        {
        }

        protected virtual void OnViewOpened(UIInputData inputData)
        {
        }

        protected virtual void OnOpenAnimationCompleted()
        {
        }

        protected virtual void OnViewClosed()
        {
        }

        #endregion

        #region Custom Methods - Private & Editor Helpers

        private async UniTaskVoid ExecuteOpenAsync()
        {
            CancelOpenAnimation();

            if (_openViewAnimations == null || _openViewAnimations.Length == 0)
            {
                ViewStatus = EViewStatus.Open;
                OnOpenAnimationCompleted();
                return;
            }

            _openCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            var ct = _openCts.Token;

            var shouldBlock = false;
            for (var i = 0; i < _openViewAnimations.Length; i++)
            {
                if (_openViewAnimations[i].BlockInteract)
                {
                    shouldBlock = true;
                    break;
                }
            }

            if (shouldBlock)
                BlockInteract();

            try
            {
                var tasks = new UniTask[_openViewAnimations.Length];
                for (var i = 0; i < _openViewAnimations.Length; i++)
                {
                    tasks[i] = _openViewAnimations[i].PlayOpen(ct);
                }

                await UniTask.WhenAll(tasks);
                ViewStatus = EViewStatus.Open;
                OnOpenAnimationCompleted();
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                if (shouldBlock)
                    UnblockInteract();
            }
        }

        private async UniTask ExecuteCloseAsync(bool ignoreAnim, CancellationToken cancellationToken)
        {
            ViewStatus = EViewStatus.Closing;
            var ct = cancellationToken == default
                ? destroyCancellationToken
                : CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, destroyCancellationToken).Token;

            var shouldBlock = false;
            if (!ignoreAnim && _closeViewAnimations != null && _closeViewAnimations.Length > 0)
            {
                for (var i = 0; i < _closeViewAnimations.Length; i++)
                {
                    if (_closeViewAnimations[i].BlockInteract)
                    {
                        shouldBlock = true;
                        break;
                    }
                }
            }

            if (shouldBlock)
                BlockInteract();

            try
            {
                if (!ignoreAnim && _closeViewAnimations != null && _closeViewAnimations.Length > 0)
                {
                    var tasks = new UniTask[_closeViewAnimations.Length];
                    for (var i = 0; i < _closeViewAnimations.Length; i++)
                    {
                        tasks[i] = _closeViewAnimations[i].PlayClose(ct);
                    }

                    await UniTask.WhenAll(tasks);
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                if (shouldBlock)
                    UnblockInteract();

                if (PauseGameStatus)
                    PauseGameHandler.Unpause(this);

                ViewStatus = EViewStatus.Closed;
                OnViewClosed();
            }
        }

        private void BlockInteract()
        {
            if (!_isInteractBlocked && UIManager.HasInstance)
            {
                _isInteractBlocked = true;
                UIManager.DisableInteract(this);
            }
        }

        private void UnblockInteract()
        {
            if (_isInteractBlocked && UIManager.HasInstance)
            {
                _isInteractBlocked = false;
                UIManager.EnableInteract(this);
            }
        }

        private void CancelOpenAnimation()
        {
            if (_openCts != null)
            {
                _openCts.Cancel();
                _openCts.Dispose();
                _openCts = null;
            }
        }

#if UNITY_EDITOR
        public void RefreshKey()
        {
            DefineKeyConstName = gameObject.name.ToValidConstKey();

            var assetPath = GetAssetPath();
            if (string.IsNullOrEmpty(assetPath))
            {
                Key = gameObject.name;
                return;
            }

            var inResources = TryGetResourcesRelativePath(assetPath, out var resourcesPath);
            var inAddressables = TryGetAddressablesAddress(assetPath, out var address);

            if (inResources && inAddressables)
            {
                Key = string.Empty;
                return;
            }

            if (inResources)
            {
                Key = resourcesPath;
                return;
            }

            if (inAddressables)
            {
                Key = address;
                return;
            }

            Key = string.Empty;
        }

        public bool ValidateKey()
        {
            DefineKeyConstName = gameObject.name.ToValidConstKey();

            var assetPath = GetAssetPath();
            if (string.IsNullOrEmpty(assetPath))
            {
                _errorMessage = string.Empty;
                return true;
            }

            var inResources = TryGetResourcesRelativePath(assetPath, out var resourcesPath);
            var inAddressables = TryGetAddressablesAddress(assetPath, out var address);

            if (inResources && inAddressables)
            {
                _errorMessage = "\u26a0 Asset cannot be both in Resources/ and marked as Addressable!";
                return false;
            }

            if (!inResources && !inAddressables)
            {
                _errorMessage = "\u26a0 Asset must be either inside a Resources/ folder or marked as Addressable!";
                return false;
            }

            if (string.IsNullOrEmpty(Key))
            {
                _errorMessage = "\u26a0 Key must not be empty!";
                return false;
            }

            var allPrefabs = UnityEditor.AssetDatabase.FindAssets("t:Prefab");
            foreach (var guid in allPrefabs)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                if (path == assetPath)
                    continue;

                var otherView = UnityEditor.AssetDatabase.LoadAssetAtPath<UIView>(path);
                if (otherView == null)
                    continue;

                var fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                if (string.Equals(fileName, gameObject.name, StringComparison.OrdinalIgnoreCase))
                {
                    _errorMessage = $"\u26a0 Duplicate UIView asset file name '{gameObject.name}' found at: {path}!";
                    return false;
                }

                if (otherView.DefineKeyConstName == DefineKeyConstName)
                {
                    _errorMessage = $"\u26a0 Duplicate define key const name '{DefineKeyConstName}' found at: {path}!";
                    return false;
                }

                if (otherView.Key == Key)
                {
                    _errorMessage = $"\u26a0 Duplicate key '{Key}' found at: {path}!";
                    return false;
                }
            }

            _errorMessage = string.Empty;
            return true;
        }

        private string GetAssetPath()
        {
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(gameObject);
            if (!string.IsNullOrEmpty(assetPath))
                return assetPath;

            var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null && stage.prefabContentsRoot == gameObject)
                return stage.assetPath;

            return UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
        }

        private static bool TryGetResourcesRelativePath(string assetPath, out string relativePath)
        {
            const string marker = "Resources/";
            var index = assetPath.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                var sub = assetPath.Substring(index + marker.Length);
                relativePath = System.IO.Path.ChangeExtension(sub, null);
                return true;
            }

            relativePath = null;
            return false;
        }

        private static bool TryGetAddressablesAddress(string assetPath, out string address)
        {
            var defaultObjectType = Type.GetType("UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject, Unity.Addressables.Editor");
            if (defaultObjectType != null)
            {
                var settingsExistsProp = defaultObjectType.GetProperty("SettingsExists", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                var settingsProp = defaultObjectType.GetProperty("Settings", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                if (settingsExistsProp != null && (bool)settingsExistsProp.GetValue(null) && settingsProp != null)
                {
                    var settings = settingsProp.GetValue(null);
                    if (settings != null)
                    {
                        var guid = UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);
                        var findEntryMethod = settings.GetType().GetMethod("FindAssetEntry", new[] { typeof(string), typeof(bool) });
                        if (findEntryMethod != null)
                        {
                            var entry = findEntryMethod.Invoke(settings, new object[] { guid, true });
                            if (entry != null)
                            {
                                var addressProp = entry.GetType().GetProperty("address");
                                if (addressProp != null)
                                {
                                    address = addressProp.GetValue(entry) as string;
                                    return !string.IsNullOrEmpty(address);
                                }
                            }
                        }
                    }
                }
            }

            address = null;
            return false;
        }
#endif

        #endregion
    }

    public abstract class UIView<TInput> : UIView where TInput : UIInputData
    {
        #region Custom Methods - Protected

        protected sealed override void OnViewOpened(UIInputData inputData)
        {
            if (inputData != null && inputData is not TInput)
            {
                Debug.LogError($"[UIView] Invalid input data type on '{name}'. Expected '{typeof(TInput).Name}', but received '{inputData.GetType().Name}'.", this);
                return;
            }

            OnViewOpened(inputData as TInput);
        }

        protected virtual void OnViewOpened(TInput inputData)
        {
        }

        #endregion
    }

    [Serializable]
    public class UIInputData
    {
        public enum EPauseGameStatus { UseDefault, Pause, NoPause }

        public EPauseGameStatus PauseStatus;
    }
}
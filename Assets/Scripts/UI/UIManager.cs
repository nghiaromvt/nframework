using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ADDRESSABLE
using UnityEngine.AddressableAssets;
#endif

namespace NFramework
{
    public enum EUILayer
    {
        Background,
        Menu,
        Popup,
        AlwaysOnTop
    }

    [RequireComponent(typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster))]
    public class UIManager : SingletonMono<UIManager>
    {
        public static event Action<BaseUIView> OnOpenedView;
        public static event Action<BaseUIView> OnClosedView;

        [SerializeField] private string _uiRootPath = "UI";

        private CanvasGroup _canvasGroup;
        private Dictionary<string, Stack<BaseUIView>> _cachedView = new Dictionary<string, Stack<BaseUIView>>();
        private Dictionary<EUILayer, List<BaseUIView>> _openedView = new Dictionary<EUILayer, List<BaseUIView>>();
        private Dictionary<EUILayer, RectTransform> _layerRectTfDict = new Dictionary<EUILayer, RectTransform>();
        private float _nextCheckTime;
        private List<object> _disableInteractRegisters = new List<object>();

        public Canvas RootCanvas { get; private set; }
        public bool CanInteract => !_canvasGroup.blocksRaycasts;

        protected override void Awake()
        {
            base.Awake();
            RootCanvas = GetComponent<Canvas>();

            var layerNames = Enum.GetNames(typeof(EUILayer));
            foreach (var name in layerNames)
            {
                var rectTf = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster)).GetComponent<RectTransform>();
                rectTf.SetParent(transform);
                rectTf.StretchFullParent();

                var uiLayer = (EUILayer)Enum.Parse(typeof(EUILayer), name);
                _layerRectTfDict[uiLayer] = rectTf;
                _openedView.Add(uiLayer, new List<BaseUIView>());
            }

            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            gameObject.SetLayerRecursively(gameObject.layer);
        }

        private void Update()
        {
            if (!CanInteract && Application.isFocused && Input.anyKeyDown && _nextCheckTime < Time.unscaledTime)
            {
                _nextCheckTime = Time.unscaledTime + 0.15f;
                var currentView = GetCurrentView();
                if (currentView != null)
                {
                    if (Input.GetKey(KeyCode.Escape))
                        currentView.HandleOnKeyBack();
                }
            }
        }

#if ADDRESSABLE
        public async Task<BaseUIView> OpenAddressable(string identifier, Action<BaseUIView> onOpened = null, bool controlInteract = true)
        {
            return await OpenAddressable<BaseUIView>(identifier, onOpened, controlInteract);
        }

        public async Task<T> OpenAddressable<T>(string identifier, Action<T> onOpened = null, bool controlInteract = true) where T : BaseUIView
        {
            if (controlInteract)
                DisableInteract(this);

            await TryCacheViewAddressable(identifier);

            if (controlInteract)
                EnableInteract(this);

            return OpenViewFromCached<T>(identifier, onOpened);
        }

        public async Task<bool> TryCacheViewAddressable(string identifier, bool forceCacheMultiple = false)
        {
            var isNew = false;
            if (!_cachedView.ContainsKey(identifier))
            {
                // New
                _cachedView[identifier] = new Stack<BaseUIView>();
                isNew = true;
            }
            else if (_cachedView[identifier].Count > 0 && !forceCacheMultiple)
            {
                // Cached and no force to cache more
                return false;
            }

            var loadPath = Path.Combine(_uiRootPath, identifier);
            var loadHandle = Addressables.LoadAssetAsync<GameObject>(loadPath);
            while (!loadHandle.IsDone)
            {
                await Task.Yield();
            }

            if (loadHandle.Result == null)
            {
                Logger.LogError($"Cannot load UI [{identifier}] from Addressable", this);
                return false;
            }

            var prefab = loadHandle.Result.GetComponent<BaseUIView>();
            if (!isNew && prefab.IsUnique)
                Logger.LogError($"UI [{identifier}] is Unique!", this);

            var cached = Instantiate(prefab, _layerRectTfDict[prefab.UILayer]);
            cached.Identifier = identifier;
            cached.gameObject.SetActive(false);
            _cachedView[identifier].Push(cached);
            return true;
        }
#endif

        public BaseUIView Open(string identifier, Action<BaseUIView> onOpened = null)
        {
            return Open<BaseUIView>(identifier, onOpened);
        }

        public T Open<T>(string identifier, Action<T> onOpened = null) where T : BaseUIView
        {
            TryCacheView(identifier);
            return OpenViewFromCached<T>(identifier, onOpened);
        }

        public async Task<BaseUIView> OpenAsync(string identifier, Action<BaseUIView> onOpened = null, bool controlInteract = true)
        {
            return await OpenAsync<BaseUIView>(identifier, onOpened, controlInteract);
        }

        public async Task<T> OpenAsync<T>(string identifier, Action<T> onOpened = null, bool controlInteract = true) where T : BaseUIView
        {
            if (controlInteract)
                DisableInteract(this);

            await TryCacheViewAsync(identifier);

            if (controlInteract)
                EnableInteract(this);

            return OpenViewFromCached<T>(identifier, onOpened);
        }

        public bool TryCacheView(string identifier, bool forceCacheMultiple = false)
        {
            var isNew = false;
            if (!_cachedView.ContainsKey(identifier))
            {
                // New
                _cachedView[identifier] = new Stack<BaseUIView>();
                isNew = true;
            }
            else if (_cachedView[identifier].Count > 0 && !forceCacheMultiple)
            {
                // Cached and no force to cache more
                return false;
            }

            var loadPath = Path.Combine(_uiRootPath, identifier);
            var prefab = Resources.Load<BaseUIView>(loadPath);
            if (!isNew && prefab.IsUnique)
                Logger.LogError($"UI [{identifier}] is Unique!", this);

            var cached = Instantiate(prefab, _layerRectTfDict[prefab.UILayer]);
            cached.Identifier = identifier;
            cached.gameObject.SetActive(false);
            _cachedView[identifier].Push(cached);
            return true;
        }

        public async Task<bool> TryCacheViewAsync(string identifier, bool forceCacheMultiple = false)
        {
            var isNew = false;
            if (!_cachedView.ContainsKey(identifier))
            {
                // New
                _cachedView[identifier] = new Stack<BaseUIView>();
                isNew = true;
            }
            else if (_cachedView[identifier].Count > 0 && !forceCacheMultiple)
            {
                // Cached and no force to cache more
                return false;
            }

            var loadPath = Path.Combine(_uiRootPath, identifier);

            var resourceRequest = Resources.LoadAsync<BaseUIView>(loadPath);
            while (!resourceRequest.isDone)
            {
                await Task.Yield();
            }

            if (resourceRequest.asset == null)
            {
                Logger.LogError($"Cannot load UI [{identifier}] from Resources", this);
                return false;
            }

            var prefab = resourceRequest.asset as BaseUIView;
            if (!isNew && prefab.IsUnique)
                Logger.LogError($"UI [{identifier}] is Unique!", this);

            var cached = Instantiate(prefab, _layerRectTfDict[prefab.UILayer]);
            cached.Identifier = identifier;
            cached.gameObject.SetActive(false);
            _cachedView[identifier].Push(cached);
            return true;
        }

        private T OpenViewFromCached<T>(string identifier, Action<T> onOpened) where T : BaseUIView
        {
            if (_cachedView[identifier].Count == 0)
            {
                Logger.LogError($"Cannot push view [{identifier}] because no cached found", this);
                return null;
            }

            var view = _cachedView[identifier].Pop() as T;
            view.gameObject.SetActive(true);
            view.transform.SetAsLastSibling();
            view.OnOpen();

            _openedView[view.UILayer].Add(view);

            onOpened?.Invoke(view);
            OnOpenedView?.Invoke(view);
            return view;
        }

        public void CloseCurrentInLayer(EUILayer layer, bool destroy = false)
        {
            var views = _openedView[layer];
            if (views.Count > 0)
                Close(views[views.Count - 1], destroy);
        }

        public void CloseAll(string identifier, bool destroy = false, List<BaseUIView> ignoreList = null)
        {
            var views = GetOpenedViewsWithIdentifier(identifier);
            foreach (var view in views)
            {
                if (!ignoreList.IsNullOrEmpty() && ignoreList.Contains(view))
                    continue;

                Close(view, destroy);
            }
        }

        public void CloseAllInLayer(EUILayer layer, bool destroy = false, List<BaseUIView> ignoreList = null)
        {
            var views = new List<BaseUIView>(_openedView[layer]);
            foreach (var view in views)
            {
                if (!ignoreList.IsNullOrEmpty() && ignoreList.Contains(view))
                    continue;

                Close(view, destroy);
            }
        }

        public bool Close(string identifier, bool destroy = false)
        {
            if (IsSpecificViewShown(identifier, out var view))
            {
                Close(view, destroy);
                return true;
            }
            return false;
        }

        public bool Close(BaseUIView view, bool destroy = false)
        {
            var views = _openedView[view.UILayer];
            if (views.Count <= 0)
                return false;

            var index = views.FindIndex((x) => x == view);
            if (index >= 0)
            {
                view.OnClose();
                views.RemoveAt(index);

                if (destroy)
                {
                    if (!view.CanDestroy)
                        Logger.LogError($"Destroy view [{view.Identifier}] with mark CanDestroy = false", this);

                    Destroy(view.gameObject);
                }
                else
                {
                    view.gameObject.SetActive(false);
                    _cachedView[view.Identifier].Push(view);
                }

                OnClosedView?.Invoke(view);
                return true;
            }
            return false;
        }

        public void DestroyAllViewCanDestroy(bool includeOpened = true, bool includeCached = true)
        {
            if (includeOpened)
            {
                var canDestroyViews = new List<BaseUIView>();
                foreach (var views in _openedView.Values)
                {
                    foreach (var view in views)
                    {
                        if (view.CanDestroy)
                            canDestroyViews.Add(view);
                    }
                }

                foreach (var view in canDestroyViews)
                    Close(view, true);
            }

            if (includeCached)
            {
                var canDestroyViews = new List<BaseUIView>();
                foreach (var cachedStack in _cachedView.Values)
                {
                    if (cachedStack.Count > 0)
                    {
                        var sample = cachedStack.Peek();
                        if (sample.CanDestroy)
                        {
                            foreach (var view in cachedStack)
                                canDestroyViews.Add(view);

                            cachedStack.Clear();
                        }
                    }
                }

                foreach (var view in canDestroyViews)
                    Destroy(view);
            }
        }

        public void OpenSystemPopupInfo(string title, string message, string ok, Action callback)
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.DisplayDialog(title, message, ok);
            callback?.Invoke();
#else
            pingak9.NativeDialog.OpenDialog(title, message, ok, callback);
#endif
        }

        public void OpenSystemPopupConfirm(string title, string message, string yes, string no, Action<bool> callback)
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorUtility.DisplayDialog(title, message, yes, no))
                callback?.Invoke(true);
            else
                callback?.Invoke(false);
#else
                pingak9.NativeDialog.OpenDialog(title, message, yes, no, () => { callback?.Invoke(true); }, () => { callback?.Invoke(false); });
#endif
        }

        public bool IsPopupShown() => _openedView[EUILayer.Popup].Count > 0;

        public bool IsSpecificViewShown(string identifier, out BaseUIView view)
        {
            view = null;
            foreach (var views in _openedView.Values)
            {
                for (int i = 0; i < views.Count; i++)
                {
                    if (views[i].Identifier == identifier)
                    {
                        view = views[i];
                        return true;
                    }
                }
            }
            return false;
        }

        public List<BaseUIView> GetOpenedViewsWithIdentifier(string identifier)
        {
            var openedViews = new List<BaseUIView>();
            foreach (var views in _openedView.Values)
            {
                foreach (var view in views)
                {
                    if (view.Identifier == identifier)
                        openedViews.Add(view);
                }
            }
            return openedViews;
        }

        public List<BaseUIView> GetAllOpenedViews()
        {
            var openedViews = new List<BaseUIView>();
            foreach (var layer in _openedView.Keys)
            {
                var views = GetOpenedViewsInLayer(layer);
                openedViews.AddRange(views);
            }
            return openedViews;
        }

        public List<BaseUIView> GetOpenedViewsInLayer(EUILayer layer)
        {
            var openedViews = new List<BaseUIView>();
            foreach (var view in _openedView[layer])
            {
                openedViews.Add(view);
            }
            return openedViews;
        }

        public BaseUIView GetCurrentView(EUILayer topLayer = EUILayer.AlwaysOnTop)
        {
            for (int i = (int)topLayer; i >= 0; --i)
            {
                var view = GetCurrentViewInLayer((EUILayer)i);
                if (view != null)
                    return view;
            }
            return null;
        }

        public BaseUIView GetCurrentViewInLayer(EUILayer layer)
        {
            var views = _openedView[layer];
            if (views.Count > 0)
                return views[views.Count - 1];
            else
                return null;
        }

        public void DisableInteract(object register = null)
        {
            if (register != null)
                _disableInteractRegisters.Add(register);

            _canvasGroup.blocksRaycasts = false;
        }

        public void EnableInteract(object register = null, bool force = false)
        {
            if (force)
            {
                _disableInteractRegisters.Clear();
            }
            else if (register != null)
            {
                if (_disableInteractRegisters.Contains(register))
                    _disableInteractRegisters.Remove(register);
                else
                    Logger.LogError($"Cannot find register: {register}");
            }

            if (_disableInteractRegisters.Count == 0)
                _canvasGroup.blocksRaycasts = true;
        }

        public bool IsPointerOverUIObject()
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return true;

            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }
    }
}



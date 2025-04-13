using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NFramework
{
    public enum EUILayer
    {
        Background = 0,
        Menu = 1,
        Popup = 2,
        Loading = 3,
        AlwaysOnTop = 4,
    }
    
    [RequireComponent(typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster))]
    public class UIManager : SingletonMono<UIManager>
    {
        [Serializable]
        private class UILayerInfo
        {
            public EUILayer layer;
            public bool overrideSorting;
            [ShowIf(nameof(overrideSorting)), ValueDropdown("SortingLayers")]  public string sortingLayer;
            [ShowIf(nameof(overrideSorting))] public int orderInLayer;
            
            private static IEnumerable SortingLayers() => SortingLayer.layers.Select(layer => layer.name).ToArray();
        }
        
        public static event Action<BaseUIView, BaseUIInputData> OnOpenedView;
        public static event Action<BaseUIView, BaseUIOutputData> OnClosedView;
        public static event Action<bool> OnInteractableChanged;

        [SerializeField] private List<UILayerInfo> _uiLayerOrders = new();
        [SerializeField] private bool _isLog = true;
        
        private static readonly Dictionary<string, Stack<BaseUIView>> _cachedView = new();
        private static readonly Dictionary<EUILayer, List<BaseUIView>> _openedView = new();
        private static readonly Dictionary<EUILayer, RectTransform> _layerRectTfDict = new();
        private static readonly List<object> _disableInteractRegisters = new();
        private static readonly List<CanvasGroup> _layerCanvasGroups = new();
        private static readonly List<string> _unloadingAddressableViewIds = new();
        private static bool _interactable = true;
        private static readonly PointerEventData _pointerEventData = new PointerEventData(EventSystem.current);
        private static readonly List<RaycastResult> _raycastResults = new();
        
        public static Canvas RootCanvas { get; private set; }

        public static bool Interactable
        {
            get => _interactable;
            private set
            {
                if (_interactable == value) return;
                _interactable = value;
                _layerCanvasGroups.ForEach(group => group.blocksRaycasts = value);
                OnInteractableChanged?.Invoke(value);
            }
        }
        
        public static Camera UICamera
        {
            get => RootCanvas.worldCamera;
            private set => RootCanvas.worldCamera = value;
        }

        protected override void Awake()
        {
            base.Awake();
            RootCanvas = GetComponent<Canvas>();

            foreach (var uiLayerInfo in _uiLayerOrders)
            {
                var rectTf = new GameObject(uiLayerInfo.layer.ToString(), typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster), typeof(CanvasGroup))
                    .GetComponent<RectTransform>();
                rectTf.SetParent(transform);
                rectTf.StretchFullParent();

                if (uiLayerInfo.overrideSorting)
                {
                    var canvas = rectTf.GetComponent<Canvas>();
                    canvas.overrideSorting = true;
                    canvas.sortingLayerName = uiLayerInfo.sortingLayer;
                    canvas.sortingOrder = uiLayerInfo.orderInLayer;
                }

                _layerRectTfDict[uiLayerInfo.layer] = rectTf;
                _openedView.Add(uiLayerInfo.layer, new List<BaseUIView>());
                _layerCanvasGroups.Add(rectTf.GetComponent<CanvasGroup>());
            }
            
            gameObject.SetLayerRecursively(gameObject.layer);
        }

        public static async UniTask<BaseUIView> Open(string id, BaseUIInputData inputData = null, bool controlInteract = true)
        {
            return await Open<BaseUIView>(id, inputData, controlInteract);
        }

        public static async UniTask<T> Open<T>(string id, BaseUIInputData inputData = null,
            bool controlInteract = true) where T : BaseUIView
        {
            T view = null;
            
            if (controlInteract) 
                DisableInteract(I);

            var hasCachedView = GetCachedViewCount(id) > 0;
            if (hasCachedView)
                view = OpenViewFromCached<T>(id);
            else
                view = await CreateAndOpenView<T>(id);

            if (view is not null)
            {
                view.transform.SetAsLastSibling();
                view.OnOpen(inputData);
                _openedView[view.UILayer].Add(view);
            }

            if (controlInteract) 
                EnableInteract(I);

            OnOpenedView?.Invoke(view, inputData);
            return view;
        }

        public static async UniTask<bool> TryCacheView(string id, bool forceCacheMultiple = false)
        {
            var curCachedViewCount = GetCachedViewCount(id);
            if (curCachedViewCount > 0 && !forceCacheMultiple)
                return false;

            await UniTask.WaitUntil(() => !_unloadingAddressableViewIds.Contains(id));

            var loadAsset = await AddressablesManager.LoadAsset<GameObject>(id);
            if (loadAsset == null)
            {
                LogError($"Cannot load UI [{id}] from Addressables");
                return false;
            }

            var prefab = loadAsset.GetComponent<BaseUIView>();
            var cached = Instantiate(prefab, _layerRectTfDict[prefab.UILayer]);
            cached.Id = id;
            cached.gameObject.SetActive(false);
            _cachedView[id].Push(cached);
            return true;
        }
        
        public static int GetCachedViewCount(string identifier)
        {
            if (!_cachedView.TryGetValue(identifier, out _))
                _cachedView[identifier] = new Stack<BaseUIView>();
            
            return _cachedView[identifier].Count;
        }

        private static T OpenViewFromCached<T>(string identifier) where T : BaseUIView
        {
            if (_cachedView[identifier].Count == 0)
            {
                LogError($"Cannot push view [{identifier}] because no cached found");
                return null;
            }

            var view = _cachedView[identifier].Pop() as T;
            view.gameObject.SetActive(true);
            return view;
        }
        
        private static async UniTask<T> CreateAndOpenView<T>(string id) where T : BaseUIView
        {
            await UniTask.WaitUntil(() => !_unloadingAddressableViewIds.Contains(id));
            
            var loadHandle = await AddressablesManager.LoadAsset<GameObject>(id);
            if (loadHandle == null)
            {
                LogError($"Cannot load UI [{id}] from Addressables");
                return null;
            }

            var prefab = loadHandle.GetComponent<T>();
            var view = Instantiate(prefab, _layerRectTfDict[prefab.UILayer]);
            view.Id = id;
            return view;
        }

        public static void CloseCurrentInLayer(EUILayer layer, bool destroy = false)
        {
            var views = _openedView[layer];
            if (views.Count > 0)
                Close(views[views.Count - 1], destroy);
        }

        public static void CloseAll(string id = null, bool destroy = false, List<BaseUIView> ignoreList = null)
        {
            var views = GetOpenedViews(id);
            foreach (var view in views)
            {
                if (!ignoreList.IsNullOrEmpty() && ignoreList.Contains(view))
                    continue;

                Close(view, destroy);
            }
        }

        public static void CloseAllInLayer(EUILayer layer, bool destroy = false, List<BaseUIView> ignoreList = null)
        {
            var views = new List<BaseUIView>(_openedView[layer]);
            foreach (var view in views)
            {
                if (!ignoreList.IsNullOrEmpty() && ignoreList.Contains(view))
                    continue;

                Close(view, destroy);
            }
        }

        public static BaseUIOutputData Close(string id, bool destroy = false)
        {
            if (IsSpecificViewShown(id, out var view))
                return Close(view, destroy);
            
            return null;
        }

        public static BaseUIOutputData Close(BaseUIView view, bool destroy = false)
        {
            var views = _openedView[view.UILayer];
            if (views.Count <= 0)
                return null;

            var index = views.FindIndex((x) => x == view);
            if (index >= 0)
            {
                views.RemoveAt(index);
                var outputData = view.OnClose();

                if (destroy)
                {
                    var id = view.Id;
                    var isFromResources = view.IsFromResources;
                    Destroy(view.gameObject);

                    if (GetCachedViewCount(id) == 0 && GetOpenedView(id) == null)
                    {
                        if (!isFromResources)
                            UnloadAddressableUI(id).Forget();
                    }
                }
                else
                {
                    view.gameObject.SetActive(false);
                    _cachedView[view.Id].Push(view);
                }

                OnClosedView?.Invoke(view, outputData);
                return outputData;
            }
            return null;
        }
        
        public static void DestroyCachedViews(string id)
        {
            var views = new List<BaseUIView>();
            foreach (var cachedStack in _cachedView.Values)
            {
                if (cachedStack.Count > 0)
                {
                    var sample = cachedStack.Peek();
                    if (sample.Id == id)
                    {
                        foreach (var view in cachedStack)
                            views.Add(view);

                        cachedStack.Clear();
                    }
                }
            }

            if (views.Count > 0)
            {
                foreach (var needDestroyViews in views)
                    Destroy(needDestroyViews.gameObject);
            }

            if (GetOpenedView(id) == null)
                UnloadAddressableUI(id).Forget();
        }

        public static async UniTask UnloadAddressableUI(string id, bool force = false)
        {
            if (_unloadingAddressableViewIds.Contains(id)) 
                return;
            
            if (GetCachedViewCount(id) > 0)
            {
                if (force) DestroyCachedViews(id);
                return;
            }

            if (GetOpenedView(id) != null)
            {
                if (force) CloseAll(id, true);
                return;
            }
            
            _unloadingAddressableViewIds.Add(id);
            Log($"UnloadAddressableUI: {id}");
            await UniTask.DelayFrame(1);
            AddressablesManager.ReleaseAsset(id);
            _unloadingAddressableViewIds.Remove(id);
        }

        public static bool IsAnyOpenedViewInLayer(EUILayer layer) => _openedView[layer].Count > 0;
        
        public static bool IsSpecificViewShown(string id, out BaseUIView view)
        {
            view = null;
            foreach (var views in _openedView.Values)
            {
                for (int i = views.Count - 1; i >= 0; i--)
                {
                    if (views[i].Id == id)
                    {
                        view = views[i];
                        return true;
                    }
                }
            }
            return false;
        }

        public static BaseUIView GetOpenedView(string id)
        {
            foreach (var views in _openedView.Values)
            {
                foreach (var view in views)
                {
                    if (view.Id == id)
                        return view;
                }
            }
            return null;
        }

        public static T GetOpenedView<T>(string id) where T : BaseUIView
        {
            var view = GetOpenedView(id);
            return view == null ? null : view as T;
        }

        public static List<BaseUIView> GetOpenedViews(string id)
        {
            var openedViews = new List<BaseUIView>();
            foreach (var views in _openedView.Values)
            {
                foreach (var view in views)
                {
                    if (id.IsNullOrEmpty() || view.Id == id)
                        openedViews.Add(view);
                }
            }
            return openedViews;
        }

        public static List<BaseUIView> GetOpenedViewsInLayer(EUILayer layer)
        {
            var openedViews = new List<BaseUIView>();
            foreach (var view in _openedView[layer])
            {
                openedViews.Add(view);
            }
            return openedViews;
        }

        public static BaseUIView GetTopmostOpenedView(EUILayer topLayer = EUILayer.AlwaysOnTop)
        {
            for (int i = (int)topLayer; i >= 0; --i)
            {
                var view = GetTopmostOpenedViewInLayer((EUILayer)i);
                if (view != null)
                    return view;
            }
            return null;
        }

        public static BaseUIView GetTopmostOpenedViewInLayer(EUILayer layer)
        {
            var views = _openedView[layer];
            if (views.Count > 0)
                return views[views.Count - 1];
            else
                return null;
        }

        [Button]
        public static void DisableInteract(object register = null)
        {
            if (register != null)
            {
                Log($"DisableInteract by: {register}");
                _disableInteractRegisters.Add(register);
            }

            Interactable = false;
        }

        [Button]
        public static void EnableInteract(object register = null, bool force = false)
        {
            if (force)
            {
                Log($"Force EnableInteract by: {register}");
                _disableInteractRegisters.Clear();
            }
            else if (register != null)
            {
                if (_disableInteractRegisters.Contains(register))
                {
                    _disableInteractRegisters.Remove(register);
                    Log($"EnableInteract by: {register}");
                }
                else
                {
                    LogError($"Cannot find register: {register}");
                }
            }

            if (_disableInteractRegisters.Count == 0)
                Interactable = true;
        }

        public static bool IsPointerOverUIObject()
        {
            // Check for UI using the current pointer (for mouse or touch)
            if (EventSystem.current == null)
                return false;

#if UNITY_EDITOR
            if (EventSystem.current.IsPointerOverGameObject())
                return true;
#endif
            
#if UNITY_ANDROID || UNITY_IOS
            if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                return true;
#endif

            // Reuse event data and result list to reduce GC alloc
            _pointerEventData.position = Input.mousePosition;
            _raycastResults.Clear();

            EventSystem.current.RaycastAll(_pointerEventData, _raycastResults);
            return _raycastResults.Count > 0;
        }

        #region Resources
        
        public static BaseUIView OpenResources(string id, BaseUIInputData inputData = null)
        {
            return OpenResources<BaseUIView>(id, inputData);
        }

        public static T OpenResources<T>(string id, BaseUIInputData inputData = null) where T : BaseUIView
        {
            T view = null;
            
            var hasCachedView = GetCachedViewCount(id) > 0;
            view = hasCachedView ? OpenViewFromCached<T>(id) : CreateAndOpenViewResources<T>(id);

            if (view is not null)
            {
                view.transform.SetAsLastSibling();
                view.OnOpen(inputData);
                _openedView[view.UILayer].Add(view);
            }
            
            OnOpenedView?.Invoke(view, inputData);
            return view;
        }
        
        private static T CreateAndOpenViewResources<T>(string id) where T : BaseUIView
        {
            var prefab = Resources.Load<BaseUIView>(id);
            if (prefab == null)
            {
                LogError($"Cannot load UI [{id}] from Resources");
                return null;
            }

            var view = Instantiate(prefab, _layerRectTfDict[prefab.UILayer]);
            view.Id = id;
            view.IsFromResources = true;
            return view as T;
        }

        public static bool TryCacheViewResources(string id, bool forceCacheMultiple = false)
        {
            var curCachedViewCount = GetCachedViewCount(id);
            if (curCachedViewCount > 0 && !forceCacheMultiple)
                return false;

            var prefab = Resources.Load<BaseUIView>(id);
            if (prefab == null)
            {
                LogError($"Cannot load UI [{id}] from Resources");
                return false;
            }
            
            var cached = Instantiate(prefab, _layerRectTfDict[prefab.UILayer]);
            cached.Id = id;
            cached.IsFromResources = true;
            cached.gameObject.SetActive(false);
            _cachedView[id].Push(cached);
            return true;
        }
        
        #endregion

        #region Log

        public static void Log(string message)
        {
            if (I._isLog) NLogger.Log(message, I, Color.blue);
        }

        public static void LogError(string message)
        {
            if (I._isLog) NLogger.LogError(message, I);
        }

        #endregion
    }
}

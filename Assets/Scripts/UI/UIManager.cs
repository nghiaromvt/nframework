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
    public enum UILayer
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
            public UILayer layer;
            public bool overrideSorting;
            [ShowIf(nameof(overrideSorting)), ValueDropdown("SortingLayers")]  public string sortingLayer;
            [ShowIf(nameof(overrideSorting))] public int orderInLayer;
            
            private static IEnumerable SortingLayers() => SortingLayer.layers.Select(layer => layer.name).ToArray();
        }
        
        public static event Action<UIView, UIInputData> OnOpenedView;
        public static event Action<UIView, UIOutputData> OnClosedView;
        public static event Action<bool> OnInteractableChanged;

        [SerializeField] private List<UILayerInfo> _uiLayerOrders = new();
        [SerializeField] private bool _isLog = true;
        [SerializeField] private string _resourcesRootFolder;
        
        private static readonly Dictionary<string, Stack<UIView>> _cachedView = new();
        private static readonly Dictionary<UILayer, List<UIView>> _openedView = new();
        private static readonly Dictionary<UILayer, RectTransform> _layerRectTfDict = new();
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
                _openedView.Add(uiLayerInfo.layer, new List<UIView>());
                _layerCanvasGroups.Add(rectTf.GetComponent<CanvasGroup>());
            }
            
            gameObject.SetLayerRecursively(gameObject.layer);
        }

#if ADDRESSABLES
        public static async UniTask<UIView> OpenAddressables(string id, UIInputData inputData = null, bool controlInteract = true)
        {
            return await OpenAddressables<UIView>(id, inputData, controlInteract);
        }

        public static async UniTask<T> OpenAddressables<T>(string id, UIInputData inputData = null,
            bool controlInteract = true) where T : UIView
        {
            T view = null;
            
            if (controlInteract) 
                DisableInteract(I);

            var hasCachedView = GetCachedViewCount(id) > 0;
            if (hasCachedView)
                view = OpenViewFromCached<T>(id);
            else
                view = await LoadAndInstantiateViewAddressables<T>(id);

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

        public static async UniTask<bool> TryCacheViewAddressables(string id, bool forceCacheMultiple = false)
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

            var prefab = loadAsset.GetComponent<UIView>();
            var cached = Instantiate(prefab, _layerRectTfDict[prefab.UILayer]);
            cached.ID = id;
            cached.gameObject.SetActive(false);
            _cachedView[id].Push(cached);
            return true;
        }
#endif
        
        public static int GetCachedViewCount(string id)
        {
            if (!_cachedView.TryGetValue(id, out _))
                _cachedView[id] = new Stack<UIView>();
            
            return _cachedView[id].Count;
        }

        private static T OpenViewFromCached<T>(string id) where T : UIView
        {
            if (_cachedView[id].Count == 0)
            {
                LogError($"Cannot push view [{id}] because no cached found");
                return null;
            }

            var view = _cachedView[id].Pop() as T;
            view.gameObject.SetActive(true);
            return view;
        }

#if ADDRESSABLES
        private static async UniTask<T> LoadAndInstantiateViewAddressables<T>(string id) where T : UIView
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
            view.ID = id;
            return view;
        }
#endif

        public static void CloseCurrentInLayer(UILayer layer, bool destroy = false)
        {
            var views = _openedView[layer];
            if (views.Count > 0)
                Close(views[^1], destroy);
        }

        public static void CloseAll(string id = null, bool destroy = false, List<UIView> ignoreList = null)
        {
            var views = GetOpenedViews(id);
            foreach (var view in views)
            {
                if (!ignoreList.IsNullOrEmpty() && ignoreList.Contains(view))
                    continue;

                Close(view, destroy);
            }
        }

        public static void CloseAllInLayer(UILayer layer, bool destroy = false, List<UIView> ignoreList = null)
        {
            var views = new List<UIView>(_openedView[layer]);
            foreach (var view in views)
            {
                if (!ignoreList.IsNullOrEmpty() && ignoreList.Contains(view))
                    continue;

                Close(view, destroy);
            }
        }

        public static UIOutputData Close(string id, bool destroy = false)
        {
            if (IsSpecificViewShown(id, out var view))
                return Close(view, destroy);
            
            return null;
        }

        public static UIOutputData Close(UIView view, bool destroy = false)
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
                    var id = view.ID;
                    var isFromResources = view.IsFromResources;
                    Destroy(view.gameObject);

                    if (GetCachedViewCount(id) == 0 && GetOpenedView(id) == null)
                    {
#if ADDRESSABLES
                        if (!isFromResources)
                            UnloadAddressableUI(id).Forget();
#endif
                    }
                }
                else
                {
                    view.gameObject.SetActive(false);
                    _cachedView[view.ID].Push(view);
                }

                OnClosedView?.Invoke(view, outputData);
                return outputData;
            }
            return null;
        }
        
        public static void DestroyCachedViews(string id)
        {
            var views = new List<UIView>();
            
            foreach (var cachedStack in _cachedView.Values)
            {
                if (cachedStack.Count > 0)
                {
                    var sample = cachedStack.Peek();
                    if (sample.ID == id)
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

#if ADDRESSABLES
            if (GetOpenedView(id) == null)
                UnloadAddressableUI(id).Forget();
#endif
        }

#if ADDRESSABLES
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
#endif

        public static bool IsAnyOpenedViewInLayer(UILayer layer) => _openedView[layer].Count > 0;
        
        public static bool IsSpecificViewShown(string id, out UIView view)
        {
            view = null;
            foreach (var views in _openedView.Values)
            {
                for (int i = views.Count - 1; i >= 0; i--)
                {
                    if (views[i].ID == id)
                    {
                        view = views[i];
                        return true;
                    }
                }
            }
            return false;
        }

        public static UIView GetOpenedView(string id)
        {
            foreach (var views in _openedView.Values)
            {
                foreach (var view in views)
                {
                    if (view.ID == id)
                        return view;
                }
            }
            return null;
        }

        public static T GetOpenedView<T>(string id) where T : UIView
        {
            var view = GetOpenedView(id);
            return view == null ? null : view as T;
        }

        public static List<UIView> GetOpenedViews(string id)
        {
            var openedViews = new List<UIView>();
            foreach (var views in _openedView.Values)
            {
                foreach (var view in views)
                {
                    if (id.IsNullOrEmpty() || view.ID == id)
                        openedViews.Add(view);
                }
            }
            return openedViews;
        }

        public static List<UIView> GetOpenedViewsInLayer(UILayer layer)
        {
            var openedViews = new List<UIView>();
            foreach (var view in _openedView[layer])
            {
                openedViews.Add(view);
            }
            return openedViews;
        }

        public static UIView GetTopmostOpenedView(UILayer topLayer = UILayer.AlwaysOnTop)
        {
            for (int i = (int)topLayer; i >= 0; --i)
            {
                var view = GetTopmostOpenedViewInLayer((UILayer)i);
                if (view != null)
                    return view;
            }
            return null;
        }

        public static UIView GetTopmostOpenedViewInLayer(UILayer layer)
        {
            var views = _openedView[layer];
            return views.Count > 0 ? views[^1] : null;
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
        
        public static UIView OpenResources(string id, UIInputData inputData = null)
        {
            return OpenResources<UIView>(id, inputData);
        }

        public static T OpenResources<T>(string id, UIInputData inputData = null) where T : UIView
        {
            T view = null;
            
            var hasCachedView = GetCachedViewCount(id) > 0;
            if (hasCachedView)
                view = OpenViewFromCached<T>(id);
            else
                view = LoadAndInstantiateViewResources<T>(id);

            if (view is not null)
            {
                view.transform.SetAsLastSibling();
                view.OnOpen(inputData);
                _openedView[view.UILayer].Add(view);
            }
            
            OnOpenedView?.Invoke(view, inputData);
            return view;
        }
        
        private static T LoadAndInstantiateViewResources<T>(string id) where T : UIView
        {
            var temp = Resources.Load<UIView>($"{I._resourcesRootFolder}{id}");
            if (temp is not T prefab)
            {
                LogError($"Cannot load UI [{id}] from Resources");
                return null;
            }
            
            var view = Instantiate(prefab, _layerRectTfDict[prefab.UILayer]);
            view.ID = id;
            view.IsFromResources = true;
            return view;
        }
        
        public static async UniTask<UIView> OpenResourcesAsync(string id, UIInputData inputData = null)
        {
            return await OpenResourcesAsync<UIView>(id, inputData);
        }

        public static async UniTask<T> OpenResourcesAsync<T>(string id, UIInputData inputData = null) where T : UIView
        {
            T view = null;
            
            var hasCachedView = GetCachedViewCount(id) > 0;
            if (hasCachedView)
                view = OpenViewFromCached<T>(id);
            else
                view = await LoadAndInstantiateViewResourcesAsync<T>(id);

            if (view is not null)
            {
                view.transform.SetAsLastSibling();
                view.OnOpen(inputData);
                _openedView[view.UILayer].Add(view);
            }
            
            OnOpenedView?.Invoke(view, inputData);
            return view;
        }
        
        private static async UniTask<T> LoadAndInstantiateViewResourcesAsync<T>(string id) where T : UIView
        {
            var temp = await Resources.LoadAsync<UIView>($"{I._resourcesRootFolder}{id}");
            if (temp is not T prefab)
            {
                LogError($"Cannot load UI [{id}] from Resources");
                return null;
            }
            
            var view = Instantiate(prefab, _layerRectTfDict[prefab.UILayer]);
            view.ID = id;
            view.IsFromResources = true;
            return view;
        }

        public static async UniTask<bool> TryCacheViewResources(string id, bool forceCacheMultiple = false)
        {
            var curCachedViewCount = GetCachedViewCount(id);
            if (curCachedViewCount > 0 && !forceCacheMultiple)
                return false;

            var temp = await Resources.LoadAsync<UIView>(id);
            if (temp is not UIView prefab)
            {
                LogError($"Cannot load UI [{id}] from Resources");
                return false;
            }
            
            var cached = Instantiate(prefab, _layerRectTfDict[prefab.UILayer]);
            cached.ID = id;
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

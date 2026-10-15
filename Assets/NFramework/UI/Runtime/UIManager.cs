using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NFramework.UI
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
    public class UIManager : PersistentSingletonMono<UIManager>
    {
        #region Nested Types

        [Serializable]
        private class UILayerInfo
        {
            public UILayer Layer;
            public bool OverrideSorting;
            [ShowIf(nameof(OverrideSorting)), ValueDropdown("SortingLayers")] public string SortingLayer;
            [ShowIf(nameof(OverrideSorting))] public int OrderInLayer;
            
            private static IEnumerable SortingLayers() => UnityEngine.SortingLayer.layers.Select(layer => layer.name).ToArray();
        }

        #endregion

        #region Events

        public static event Action<UIView, UIInputData> OnOpenedView;
        public static event Action<UIView> OnClosedView;
        public static event Action<bool> OnInteractableChanged;

        #endregion

        #region Fields

        [SerializeField] private List<UILayerInfo> _uiLayerOrders = new();
        
        private readonly Dictionary<string, Stack<UIView>> _cachedView = new();
        private readonly Dictionary<UILayer, List<UIView>> _openedView = new();
        private readonly Dictionary<UILayer, RectTransform> _layerRectTfDict = new();
        private readonly List<object> _disableInteractRegisters = new();
        private readonly List<CanvasGroup> _layerCanvasGroups = new();
        private readonly HashSet<string> _unloadingAddressableViewIds = new();
#if ADDRESSABLES
        private readonly Dictionary<string, AssetHandle<GameObject>> _addressablePrefabHandles = new();
#endif
        private bool _interactable = true;
        private PointerEventData _pointerEventData;
        private readonly List<RaycastResult> _raycastResults = new();
        private Canvas _rootCanvas;

        #endregion

        #region Properties

        public Canvas RootCanvas
        {
            get
            {
                if (_rootCanvas == null)
                    _rootCanvas = GetComponent<Canvas>();
                
                return _rootCanvas;
            }
        }

        public static bool Interactable
        {
            get => I._interactable;
            private set
            {
                if (I._interactable == value) return;
                I._interactable = value;
                I._layerCanvasGroups.ForEach(group => group.blocksRaycasts = value);
                OnInteractableChanged?.Invoke(value);
            }
        }
        
        public static Camera UICamera
        {
            get => I.RootCanvas.worldCamera;
            set => I.RootCanvas.worldCamera = value;
        }

        #endregion

        #region Unity Lifecycle Functions

        protected override void Awake()
        {
            base.Awake();

            foreach (var uiLayerInfo in _uiLayerOrders)
            {
                var rectTf = new GameObject(uiLayerInfo.Layer.ToString(), typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster), typeof(CanvasGroup))
                    .GetComponent<RectTransform>();
                rectTf.SetParent(transform);
                rectTf.StretchFullParent();

                if (uiLayerInfo.OverrideSorting)
                {
                    var canvas = rectTf.GetComponent<Canvas>();
                    canvas.overrideSorting = true;
                    canvas.sortingLayerName = uiLayerInfo.SortingLayer;
                    canvas.sortingOrder = uiLayerInfo.OrderInLayer;
                }

                I._layerRectTfDict[uiLayerInfo.Layer] = rectTf;
                I._openedView.Add(uiLayerInfo.Layer, new List<UIView>());
                I._layerCanvasGroups.Add(rectTf.GetComponent<CanvasGroup>());
            }
            
            gameObject.SetLayerRecursively(gameObject.layer);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_uiLayerOrders == null || _uiLayerOrders.Count == 0)
            {
                _uiLayerOrders = new List<UILayerInfo>();
                foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
                    _uiLayerOrders.Add(new UILayerInfo { Layer = layer });
            }
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            OnOpenedView = null;
            OnClosedView = null;
            OnInteractableChanged = null;
        }

        #endregion

        #region Custom Methods - Open View

#if ADDRESSABLES
        public static async UniTask<UIView> OpenAddressables(string id, UIInputData inputData = null, bool controlInteract = true, CancellationToken cancellationToken = default)
        {
            return await OpenAddressables<UIView>(id, inputData, controlInteract, cancellationToken);
        }

        public static async UniTask<T> OpenAddressables<T>(string id, UIInputData inputData = null,
            bool controlInteract = true, CancellationToken cancellationToken = default) where T : UIView
        {
            if (string.IsNullOrEmpty(id))
            {
                LogError($"OpenAddressables failed: 'id' cannot be null or empty for view type {typeof(T).Name}");
                return null;
            }

            if (TryGetOpenedMenu(id, out var openedMenu))
            {
                Log($"Menu [{id}] is already opened or opening. Skipping OpenAddressables.");
                return openedMenu as T;
            }

            var ct = ResolveToken(cancellationToken);
            T view = null;
            
            if (controlInteract) 
                DisableInteract(I);
        
            var hasCachedView = GetCachedViewCount(id) > 0;
            if (hasCachedView)
                view = OpenViewFromCached<T>(id);
            else
                view = await LoadAndInstantiateViewAddressables<T>(id, ct);
        
            FinalizeOpen(view, inputData);
        
            if (controlInteract) 
                EnableInteract(I);
            
            return view;
        }
#endif

        public static UIView OpenResources(string id, UIInputData inputData = null)
        {
            return OpenResources<UIView>(id, inputData);
        }

        public static T OpenResources<T>(string id, UIInputData inputData = null) where T : UIView
        {
            if (string.IsNullOrEmpty(id))
            {
                LogError($"OpenResources failed: 'id' cannot be null or empty for view type {typeof(T).Name}");
                return null;
            }

            if (TryGetOpenedMenu(id, out var openedMenu))
            {
                Log($"Menu [{id}] is already opened or opening. Skipping OpenResources.");
                return openedMenu as T;
            }

            T view = null;
            
            var hasCachedView = GetCachedViewCount(id) > 0;
            if (hasCachedView)
                view = OpenViewFromCached<T>(id);
            else
                view = LoadAndInstantiateViewResources<T>(id);

            FinalizeOpen(view, inputData);

            return view;
        }

        public static async UniTask<UIView> OpenResourcesAsync(string id, UIInputData inputData = null, bool controlInteract = true, CancellationToken cancellationToken = default)
        {
            return await OpenResourcesAsync<UIView>(id, inputData, controlInteract, cancellationToken);
        }

        public static async UniTask<T> OpenResourcesAsync<T>(string id, UIInputData inputData = null,
            bool controlInteract = true, CancellationToken cancellationToken = default) where T : UIView
        {
            if (string.IsNullOrEmpty(id))
            {
                LogError($"OpenResourcesAsync failed: 'id' cannot be null or empty for view type {typeof(T).Name}");
                return null;
            }

            if (TryGetOpenedMenu(id, out var openedMenu))
            {
                Log($"Menu [{id}] is already opened or opening. Skipping OpenResourcesAsync.");
                return openedMenu as T;
            }

            var ct = ResolveToken(cancellationToken);
            T view = null;
            
            if (controlInteract)
                DisableInteract(I);
            
            var hasCachedView = GetCachedViewCount(id) > 0;
            if (hasCachedView)
                view = OpenViewFromCached<T>(id);
            else
                view = await LoadAndInstantiateViewResourcesAsync<T>(id, ct);

            FinalizeOpen(view, inputData);
            
            if (controlInteract)
                EnableInteract(I);
            
            return view;
        }

        #endregion

        #region Custom Methods - Close View

        public static async UniTask Close(string id, bool destroy = false, bool ignoreAnim = false, CancellationToken cancellationToken = default)
        {
            if (IsSpecificViewShown(id, out var view))
            {
                await Close(view, destroy, ignoreAnim, cancellationToken);
                return;
            }
            
            Log($"Close failed: view [{id}] is not currently open");
        }

        public static async UniTask Close(UIView view, bool destroy = false, bool ignoreAnim = false, CancellationToken cancellationToken = default)
        {
            if (view == null) return;

            if (!I._openedView.TryGetValue(view.UILayer, out var views))
            {
                LogError($"Close failed: layer [{view.UILayer}] is not configured in UIManager");
                return;
            }

            var ct = ResolveToken(cancellationToken);
            await view.OnClose(ignoreAnim, ct);

            views.Remove(view);
            OnClosedView?.Invoke(view);

            FinalizeClose(view, destroy);
        }

        private static void FinalizeClose(UIView view, bool destroy)
        {
            if (view == null) return;

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
                I._cachedView[view.ID].Push(view);
            }
        }

        public static async UniTask CloseCurrentInLayer(UILayer layer, bool destroy = false, bool ignoreAnim = false, CancellationToken cancellationToken = default)
        {
            var views = I._openedView[layer];
            if (views.Count > 0)
                await Close(views[^1], destroy, ignoreAnim, cancellationToken);
        }

        public static async UniTask CloseAll(string id = null, bool destroy = false, bool ignoreAnim = false, List<UIView> ignoreList = null, CancellationToken cancellationToken = default)
        {
            var views = GetOpenedViews(id);
            var tasks = new List<UniTask>();

            foreach (var view in views)
            {
                if (!ignoreList.IsNullOrEmpty() && ignoreList.Contains(view))
                    continue;

                tasks.Add(Close(view, destroy, ignoreAnim, cancellationToken));
            }

            if (tasks.Count > 0)
                await UniTask.WhenAll(tasks);
        }

        public static async UniTask CloseAllInLayer(UILayer layer, bool destroy = false, bool ignoreAnim = false, List<UIView> ignoreList = null, CancellationToken cancellationToken = default)
        {
            var views = new List<UIView>(I._openedView[layer]);
            var tasks = new List<UniTask>();

            foreach (var view in views)
            {
                if (!ignoreList.IsNullOrEmpty() && ignoreList.Contains(view))
                    continue;

                tasks.Add(Close(view, destroy, ignoreAnim, cancellationToken));
            }

            if (tasks.Count > 0)
                await UniTask.WhenAll(tasks);
        }

        #endregion

        #region Custom Methods - Cache View

        public static int GetCachedViewCount(string id)
        {
            if (!I._cachedView.TryGetValue(id, out _))
                I._cachedView[id] = new Stack<UIView>();
            
            return I._cachedView[id].Count;
        }

#if ADDRESSABLES
        public static async UniTask<bool> TryCacheViewAddressables(string id, bool forceCacheMultiple = false, CancellationToken cancellationToken = default)
        {
            var ct = ResolveToken(cancellationToken);
            var curCachedViewCount = GetCachedViewCount(id);
            if (curCachedViewCount > 0 && !forceCacheMultiple)
                return false;
        
            await UniTask.WaitUntil(() => !I._unloadingAddressableViewIds.Contains(id), cancellationToken: ct);
        
            if (!I._addressablePrefabHandles.TryGetValue(id, out var handle))
            {
                handle = await AddressablesLoader.LoadAsset<GameObject>(id, cancellationToken: ct);
                if (handle == null || handle.Result == null)
                {
                    LogError($"Cannot load UI [{id}] from Addressables");
                    return false;
                }

                I._addressablePrefabHandles[id] = handle;
            }
        
            var prefab = handle.Result.GetComponent<UIView>();
            var cached = Instantiate(prefab, I._layerRectTfDict[prefab.UILayer]);
            cached.Initialize(id);
            cached.gameObject.SetActive(false);
            I._cachedView[id].Push(cached);
            return true;
        }
#endif

        public static async UniTask<bool> TryCacheViewResources(string id, bool forceCacheMultiple = false, CancellationToken cancellationToken = default)
        {
            var ct = ResolveToken(cancellationToken);
            var curCachedViewCount = GetCachedViewCount(id);
            if (curCachedViewCount > 0 && !forceCacheMultiple)
                return false;

            var request = Resources.LoadAsync<UIView>(id);
            var temp = await request.WithCancellation(ct);
            if (temp is not UIView prefab)
            {
                LogError($"Cannot load UI [{id}] from Resources");
                return false;
            }
            
            var cached = Instantiate(prefab, I._layerRectTfDict[prefab.UILayer]);
            cached.Initialize(id, isFromResources: true);
            cached.gameObject.SetActive(false);
            I._cachedView[id].Push(cached);
            return true;
        }

        public static void DestroyCachedViews(string id)
        {
            if (!I._cachedView.TryGetValue(id, out var stack) || stack.Count == 0)
                return;

            var isFromResources = stack.Peek().IsFromResources;
            var views = new List<UIView>(stack);
            stack.Clear();

            foreach (var view in views)
                Destroy(view.gameObject);

#if ADDRESSABLES
            if (!isFromResources && GetOpenedView(id) == null)
                UnloadAddressableUI(id).Forget();
#endif
        }

        #endregion

        #region Custom Methods - Query View

        private static bool TryGetOpenedMenu(string id, out UIView menu)
        {
            menu = null;
            if (!I._openedView.TryGetValue(UILayer.Menu, out var menuViews))
                return false;

            for (int i = menuViews.Count - 1; i >= 0; i--)
            {
                var view = menuViews[i];
                if (view.ID == id && view.ViewStatus is EViewStatus.Open or EViewStatus.Opening)
                {
                    menu = view;
                    return true;
                }
            }

            return false;
        }

        public static bool IsSpecificViewShown(string id, out UIView view)
        {
            view = GetOpenedView(id);
            return view != null;
        }

        public static bool IsAnyOpenedViewInLayer(UILayer layer) => I._openedView[layer].Count > 0;

        public static UIView GetOpenedView(string id)
        {
            foreach (var views in I._openedView.Values)
            {
                for (int i = views.Count - 1; i >= 0; i--)
                {
                    if (views[i].ID == id)
                        return views[i];
                }
            }
            return null;
        }

        public static T GetOpenedView<T>(string id = null) where T : UIView
        {
            id = string.IsNullOrEmpty(id) ? typeof(T).Name : id;
            var view = GetOpenedView(id);
            return view == null ? null : view as T;
        }

        public static List<UIView> GetOpenedViews(string id)
        {
            var openedViews = new List<UIView>();
            foreach (var views in I._openedView.Values)
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
            foreach (var view in I._openedView[layer])
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
            var views = I._openedView[layer];
            return views.Count > 0 ? views[^1] : null;
        }

        #endregion

        #region Custom Methods - Interact Control

        [Button]
        public static void DisableInteract(object register = null)
        {
            if (register != null)
            {
                Log($"DisableInteract by: {register}");
                if (!I._disableInteractRegisters.Contains(register))
                    I._disableInteractRegisters.Add(register);
            }

            Interactable = false;
        }

        [Button]
        public static void EnableInteract(object register = null, bool force = false)
        {
            if (force)
            {
                Log($"Force EnableInteract by: {register}");
                I._disableInteractRegisters.Clear();
            }
            else if (register != null)
            {
                if (I._disableInteractRegisters.Contains(register))
                {
                    I._disableInteractRegisters.Remove(register);
                    Log($"EnableInteract by: {register}");
                }
                else
                {
                    LogError($"Cannot find register: {register}");
                }
            }

            if (I._disableInteractRegisters.Count == 0)
                Interactable = true;
        }

        #endregion

        #region Custom Methods - Utilities

        public static bool IsPointerOverUIObject()
        {
            if (EventSystem.current == null)
                return false;

            // 1. Direct check via EventSystem
#if ENABLE_INPUT_SYSTEM
            if (Touchscreen.current != null)
            {
                var touches = Touchscreen.current.touches;
                for (int i = 0; i < touches.Count; i++)
                {
                    if (touches[i].isInProgress && EventSystem.current.IsPointerOverGameObject(touches[i].touchId.ReadValue()))
                        return true;
                }
            }
            
            if (EventSystem.current.IsPointerOverGameObject())
                return true;
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                return true;

            if (EventSystem.current.IsPointerOverGameObject())
                return true;
#else
            if (EventSystem.current.IsPointerOverGameObject())
                return true;
#endif

            // 2. Graphic Raycast fallback
            Vector2 screenPosition;

#if ENABLE_INPUT_SYSTEM
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
                screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            else if (Mouse.current != null)
                screenPosition = Mouse.current.position.ReadValue();
            else
                return false;
#elif ENABLE_LEGACY_INPUT_MANAGER
            screenPosition = Input.mousePosition;
#else
            return false;
#endif

            I._pointerEventData ??= new PointerEventData(EventSystem.current);
            I._pointerEventData.position = screenPosition;
            I._raycastResults.Clear();

            EventSystem.current.RaycastAll(I._pointerEventData, I._raycastResults);
            return I._raycastResults.Count > 0;
        }

#if ADDRESSABLES
        public static async UniTask UnloadAddressableUI(string id, bool force = false, CancellationToken cancellationToken = default)
        {
            var ct = ResolveToken(cancellationToken);
            if (I._unloadingAddressableViewIds.Contains(id)) 
                return;
            
            if (GetCachedViewCount(id) > 0)
            {
                if (force) DestroyCachedViews(id);
                return;
            }
        
            if (GetOpenedView(id) != null)
            {
                if (force) await CloseAll(id, true, cancellationToken: ct);
                return;
            }
            
            I._unloadingAddressableViewIds.Add(id);
            Log($"UnloadAddressableUI: {id}");
            await UniTask.DelayFrame(1, cancellationToken: ct);
            if (I._addressablePrefabHandles.Remove(id, out var handle))
            {
                handle.Dispose();
            }
            I._unloadingAddressableViewIds.Remove(id);
        }
#endif

        #endregion

        #region Custom Methods - Private Helpers

        private static CancellationToken ResolveToken(CancellationToken token)
        {
            return token == default 
                ? I.destroyCancellationToken 
                : CancellationTokenSource.CreateLinkedTokenSource(token, I.destroyCancellationToken).Token;
        }

        private static void FinalizeOpen(UIView view, UIInputData inputData)
        {
            if (view == null) return;

            view.transform.SetAsLastSibling();
            view.OnOpen(inputData);
            I._openedView[view.UILayer].Add(view);
            OnOpenedView?.Invoke(view, inputData);
        }

        private static T OpenViewFromCached<T>(string id) where T : UIView
        {
            if (I._cachedView[id].Count == 0)
            {
                LogError($"Cannot push view [{id}] because no cached found");
                return null;
            }

            var view = I._cachedView[id].Pop() as T;
            view.gameObject.SetActive(true);
            return view;
        }

#if ADDRESSABLES
        private static async UniTask<T> LoadAndInstantiateViewAddressables<T>(string id, CancellationToken cancellationToken) where T : UIView
        {
            await UniTask.WaitUntil(() => !I._unloadingAddressableViewIds.Contains(id), cancellationToken: cancellationToken);
            
            if (!I._addressablePrefabHandles.TryGetValue(id, out var handle))
            {
                handle = await AddressablesLoader.LoadAsset<GameObject>(id, cancellationToken: cancellationToken);
                if (handle == null || handle.Result == null)
                {
                    LogError($"Cannot load UI [{id}] from Addressables");
                    return null;
                }

                I._addressablePrefabHandles[id] = handle;
            }
        
            var prefab = handle.Result.GetComponent<T>();
            var view = Instantiate(prefab, I._layerRectTfDict[prefab.UILayer]);
            view.Initialize(id);
            return view;
        }
#endif

        private static T LoadAndInstantiateViewResources<T>(string id) where T : UIView
        {
            var temp = Resources.Load<UIView>(id);
            if (temp is not T prefab)
            {
                LogError($"Cannot load UI [{id}] from Resources");
                return null;
            }
            
            var view = Instantiate(prefab, I._layerRectTfDict[prefab.UILayer]);
            view.Initialize(id, isFromResources: true);
            return view;
        }

        private static async UniTask<T> LoadAndInstantiateViewResourcesAsync<T>(string id, CancellationToken cancellationToken) where T : UIView
        {
            var request = Resources.LoadAsync<UIView>(id);
            var temp = await request.WithCancellation(cancellationToken);
            if (temp is not T prefab)
            {
                LogError($"Cannot load UI [{id}] from Resources");
                return null;
            }
            
            var view = Instantiate(prefab, I._layerRectTfDict[prefab.UILayer]);
            view.Initialize(id, isFromResources: true);
            return view;
        }

        #endregion

        #region Custom Methods - Logging

        public static void Log(string message)
        {
            NLogger.Log(message, I, Color.blue);
        }

        public static void LogError(string message)
        {
            NLogger.LogError(message, I);
        }

        #endregion
    }
}

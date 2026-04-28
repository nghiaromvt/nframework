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
        #region Nested Types

        [Serializable]
        private class UILayerInfo
        {
            public UILayer layer;
            public bool overrideSorting;
            [ShowIf(nameof(overrideSorting)), ValueDropdown("SortingLayers")]  public string sortingLayer;
            [ShowIf(nameof(overrideSorting))] public int orderInLayer;
            
            private static IEnumerable SortingLayers() => SortingLayer.layers.Select(layer => layer.name).ToArray();
        }

        #endregion

        #region Events

        public static event Action<UIView, UIInputData> OnOpenedView;
        public static event Action<UIView, UIOutputData> OnClosedView;
        public static event Action<bool> OnInteractableChanged;

        #endregion

        #region Variables

        [SerializeField] private List<UILayerInfo> _uiLayerOrders = new();
        [SerializeField] private bool _isLog = true;
        [SerializeField] private string _resourcesRootFolder;
        [SerializeField] private string _refPathAddressable;
        
        private readonly Dictionary<string, Stack<UIView>> _cachedView = new();
        private readonly Dictionary<UILayer, List<UIView>> _openedView = new();
        private readonly Dictionary<UILayer, RectTransform> _layerRectTfDict = new();
        private readonly List<object> _disableInteractRegisters = new();
        private readonly List<CanvasGroup> _layerCanvasGroups = new();
        private readonly HashSet<string> _unloadingAddressableViewIds = new();
        private bool _interactable = true;
        private PointerEventData _pointerEventData;
        private readonly List<RaycastResult> _raycastResults = new();

        #endregion

        #region Properties

        public Canvas RootCanvas { get; private set; }

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

        #region Unity Functions

        protected override void Awake()
        {
            base.Awake();
            RootCanvas = GetComponent<Canvas>();
            _pointerEventData = new PointerEventData(EventSystem.current);

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

                I._layerRectTfDict[uiLayerInfo.layer] = rectTf;
                I._openedView.Add(uiLayerInfo.layer, new List<UIView>());
                I._layerCanvasGroups.Add(rectTf.GetComponent<CanvasGroup>());
            }
            
            gameObject.SetLayerRecursively(gameObject.layer);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            OnOpenedView = null;
            OnClosedView = null;
            OnInteractableChanged = null;
        }

        #endregion

        #region Public - Open View

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
            T view = null;
            
            var hasCachedView = GetCachedViewCount(id) > 0;
            if (hasCachedView)
                view = OpenViewFromCached<T>(id);
            else
                view = LoadAndInstantiateViewResources<T>(id);

            FinalizeOpen(view, inputData);

            return view;
        }

        public static async UniTask<UIView> OpenResourcesAsync(string id, UIInputData inputData = null, bool controlInteract = true)
        {
            return await OpenResourcesAsync<UIView>(id, inputData, controlInteract);
        }

        public static async UniTask<T> OpenResourcesAsync<T>(string id, UIInputData inputData = null,
            bool controlInteract = true) where T : UIView
        {
            T view = null;
            
            if (controlInteract)
                DisableInteract(I);
            
            var hasCachedView = GetCachedViewCount(id) > 0;
            if (hasCachedView)
                view = OpenViewFromCached<T>(id);
            else
                view = await LoadAndInstantiateViewResourcesAsync<T>(id);

            FinalizeOpen(view, inputData);
            
            if (controlInteract)
                EnableInteract(I);
            
            return view;
        }

        #endregion

        #region Public - Close View

        public static UIOutputData Close(string id, bool destroy = false)
        {
            if (IsSpecificViewShown(id, out var view))
                return Close(view, destroy);
            
            Log($"Close failed: view [{id}] is not currently open");
            return null;
        }

        public static UIOutputData Close(UIView view, bool destroy = false)
        {
            var views = I._openedView[view.UILayer];
            if (views.Count <= 0)
                return null;

            var index = views.FindIndex((x) => x == view);
            if (index >= 0)
            {
                views.RemoveAt(index);
                var outputData = view.OnClose();
                OnClosedView?.Invoke(view, outputData);

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
                    if (view != null)
                    {
                        view.gameObject.SetActive(false);
                        I._cachedView[view.ID].Push(view);
                    }
                }

                return outputData;
            }
            return null;
        }

        public static void CloseCurrentInLayer(UILayer layer, bool destroy = false)
        {
            var views = I._openedView[layer];
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
            var views = new List<UIView>(I._openedView[layer]);
            foreach (var view in views)
            {
                if (!ignoreList.IsNullOrEmpty() && ignoreList.Contains(view))
                    continue;

                Close(view, destroy);
            }
        }

        #endregion

        #region Public - Cache View

        public static int GetCachedViewCount(string id)
        {
            if (!I._cachedView.TryGetValue(id, out _))
                I._cachedView[id] = new Stack<UIView>();
            
            return I._cachedView[id].Count;
        }

#if ADDRESSABLES
        public static async UniTask<bool> TryCacheViewAddressables(string id, bool forceCacheMultiple = false)
        {
            var curCachedViewCount = GetCachedViewCount(id);
            if (curCachedViewCount > 0 && !forceCacheMultiple)
                return false;
        
            await UniTask.WaitUntil(() => !I._unloadingAddressableViewIds.Contains(id), cancellationToken: I.destroyCancellationToken);
        
            var loadAsset = await AddressablesManager.LoadAsset<GameObject>(GetViewPfAddressablesPath(id));
            if (loadAsset == null)
            {
                LogError($"Cannot load UI [{id}] from Addressables");
                return false;
            }
        
            var prefab = loadAsset.GetComponent<UIView>();
            var cached = Instantiate(prefab, I._layerRectTfDict[prefab.UILayer]);
            cached.Initialize(id);
            cached.gameObject.SetActive(false);
            I._cachedView[id].Push(cached);
            return true;
        }
#endif

        public static async UniTask<bool> TryCacheViewResources(string id, bool forceCacheMultiple = false)
        {
            var curCachedViewCount = GetCachedViewCount(id);
            if (curCachedViewCount > 0 && !forceCacheMultiple)
                return false;

            var temp = await Resources.LoadAsync<UIView>(GetViewPfResourcesPath(id));
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

        #region Public - Query View

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

        public static T GetOpenedView<T>(string id) where T : UIView
        {
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

        #region Public - Interact Control

        [Button]
        public static void DisableInteract(object register = null)
        {
            if (register != null)
            {
                Log($"DisableInteract by: {register}");
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

        #region Public - Utilities

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
            I._pointerEventData.position = Input.mousePosition;
            I._raycastResults.Clear();

            EventSystem.current.RaycastAll(I._pointerEventData, I._raycastResults);
            return I._raycastResults.Count > 0;
        }

#if ADDRESSABLES
        public static async UniTask UnloadAddressableUI(string id, bool force = false)
        {
            if (I._unloadingAddressableViewIds.Contains(id)) 
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
            
            I._unloadingAddressableViewIds.Add(id);
            Log($"UnloadAddressableUI: {id}");
            await UniTask.DelayFrame(1, cancellationToken: I.destroyCancellationToken);
            AddressablesManager.ReleaseAsset(GetViewPfAddressablesPath(id));
            I._unloadingAddressableViewIds.Remove(id);
        }
#endif

        #endregion

        #region Private Functions

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
        private static async UniTask<T> LoadAndInstantiateViewAddressables<T>(string id) where T : UIView
        {
            await UniTask.WaitUntil(() => !_unloadingAddressableViewIds.Contains(id), cancellationToken: I.destroyCancellationToken);
            
            var loadHandle = await AddressablesManager.LoadAsset<GameObject>(GetViewPfAddressablesPath(id));
            if (loadHandle == null)
            {
                LogError($"Cannot load UI [{id}] from Addressables");
                return null;
            }
        
            var prefab = loadHandle.GetComponent<T>();
            var view = Instantiate(prefab, I._layerRectTfDict[prefab.UILayer]);
            view.Initialize(id);
            return view;
        }

        private static string GetViewPfAddressablesPath(string id) => $"{I._refPathAddressable}/{id}.prefab";
#endif

        private static T LoadAndInstantiateViewResources<T>(string id) where T : UIView
        {
            var temp = Resources.Load<UIView>(GetViewPfResourcesPath(id));
            if (temp is not T prefab)
            {
                LogError($"Cannot load UI [{id}] from Resources");
                return null;
            }
            
            var view = Instantiate(prefab, I._layerRectTfDict[prefab.UILayer]);
            view.Initialize(id, isFromResources: true);
            return view;
        }

        private static async UniTask<T> LoadAndInstantiateViewResourcesAsync<T>(string id) where T : UIView
        {
            var temp = await Resources.LoadAsync<UIView>(GetViewPfResourcesPath(id));
            if (temp is not T prefab)
            {
                LogError($"Cannot load UI [{id}] from Resources");
                return null;
            }
            
            var view = Instantiate(prefab, I._layerRectTfDict[prefab.UILayer]);
            view.Initialize(id, isFromResources: true);
            return view;
        }

        private static string GetViewPfResourcesPath(string id) => $"{I._resourcesRootFolder}{id}";

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

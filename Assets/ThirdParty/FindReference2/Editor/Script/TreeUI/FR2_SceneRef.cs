using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace vietlabs.fr2
{
    internal class FR2_SceneRef : FR2_Ref
    {
        internal static readonly Dictionary<string, Type> CacheType = new Dictionary<string, Type>();
        private static readonly Dictionary<Type, string> typeNameCache = new Dictionary<Type, string>();
        private static readonly Dictionary<string, string> beautifiedPathCache = new Dictionary<string, string>();

        private static Action<Dictionary<string, FR2_Ref>> onFindRefInSceneComplete;
        private static Dictionary<string, FR2_Ref> refs = new Dictionary<string, FR2_Ref>();
        private static string[] cacheAssetGuids;
        private GUIContent assetNameGC;
        private GUIContent assetTypeGC;

        public Func<bool> drawFullPath;
        public string sceneFullPath = "";
        public string scenePath = "";
        public string targetType;
        
        public List<SceneRefInfo> sourceRefs;
        public List<SceneRefInfo> backwardRefs;

        public FR2_SceneRef(int index, int depth, FR2_Asset asset, FR2_Asset by) : base(index, depth, asset, by)
        {
            isSceneRef = true;
            sourceRefs = new List<SceneRefInfo>();
            backwardRefs = new List<SceneRefInfo>();
            // Ensure tooltip always shows full path with proper slashes
            string tooltipPath = asset?.assetPath ?? "Unknown";
            assetNameGC = FR2_GUIContent.FromString(asset?.assetName ?? "Unknown", tooltipPath);
            assetTypeGC = FR2_GUIContent.FromString("");
        }
        
        public FR2_SceneRef(int depth, UnityObject target) : base(0, depth, null, null)
        {
            component = target;
            this.depth = depth;
            isSceneRef = true;
            sourceRefs = new List<SceneRefInfo>();
            backwardRefs = new List<SceneRefInfo>();
            InitializeTargetInfo(target);
        }

        void InitializeTargetInfo(UnityObject target)
        {
            if (target == null)
            {
                targetType = "Missing";
                scenePath = "";
                sceneFullPath = "Missing Object";
                assetNameGC = FR2_GUIContent.FromString("Missing Object", "Object has been destroyed");
                assetTypeGC = FR2_GUIContent.FromString("Missing");
                return;
            }

            if (target is GameObject obj)
            {
                targetType = nameof(GameObject);
                scenePath = FR2_Unity.GetGameObjectPath(obj, false);
                // Add trailing slash if scenePath is not empty
                string pathWithSlash = string.IsNullOrEmpty(scenePath) ? "" : scenePath + "/";
                sceneFullPath = pathWithSlash + obj.name;
                assetNameGC = FR2_GUIContent.FromString(obj.name, sceneFullPath);
                assetTypeGC = GUIContent.none;
            }
            else if (target is Component com)
            {
                targetType = GetCachedTypeName(component.GetType());
                scenePath = FR2_Unity.GetGameObjectPath(com.gameObject, false);
                // Add trailing slash if scenePath is not empty
                string pathWithSlash = string.IsNullOrEmpty(scenePath) ? "" : scenePath + "/";
                sceneFullPath = pathWithSlash + com.gameObject.name;
                assetNameGC = FR2_GUIContent.FromString(com.gameObject.name, sceneFullPath);
                assetTypeGC = FR2_GUIContent.FromString(GetCachedTypeName(component.GetType()));
            }
        }

        static string GetCachedTypeName(Type type)
        {
            if (typeNameCache.TryGetValue(type, out string cachedName)) return cachedName;
            cachedName = type.Name;
            typeNameCache.Add(type, cachedName);
            return cachedName;
        }

        public override bool isSelected()
        {
            // SceneObjects should never be considered bookmarked/selected
            return false;
        }


        public void Draw(Rect r, FR2_RefDrawer.Mode groupMode, bool showDetails, bool drawFullPath = false)
        {
            // r.xMin -= 12f;
            r.xMax -= 4f;
            
            var margin = 2;
            var pingRect = r;
            Rect iconRect = GUI2.LeftRect(16f, ref r);
            
            // Right-click context menu for scene objects
            if ((Event.current.type == EventType.MouseUp) && (Event.current.button == 1) && pingRect.Contains(Event.current.mousePosition))
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Open"), false, () => { if (component != null) EditorGUIUtility.PingObject(component); });
                menu.AddItem(new GUIContent("Ping"), false, () => { if (component != null) EditorGUIUtility.PingObject(component); });
#if UNITY_2022_3_OR_NEWER
                menu.AddItem(new GUIContent("Properties..."), false, () => { if (component != null) EditorUtility.OpenPropertyEditor(component); });
#else
                menu.AddDisabledItem(new GUIContent("Properties..."));
#endif
                menu.ShowAsContext();
                Event.current.Use();
            }
            
            var (icon, iconTooltip) = GetTargetIcon();
            
            // Calculate available space for reference icons first (reserve space on the right)
            float referenceIconsWidth = CalculateReferenceIconsWidth();
            float availableWidth = r.width - referenceIconsWidth - 4f; // 4f for margin
            
            // Calculate sizes based on whether we're showing full path
            float pathW = 0f;
            float nameW = 0f;
            float typeW = EditorStyles.miniLabel.CalcSize(assetTypeGC).x;
            
            if (drawFullPath && !string.IsNullOrEmpty(scenePath))
            {
                var pathContent = FR2_GUIContent.FromString(scenePath + "/");
                pathW = Mathf.Min(EditorStyles.miniLabel.CalcSize(pathContent).x, availableWidth * 0.4f); // Max 40% for path
                nameW = Mathf.Min(EditorStyles.label.CalcSize(assetNameGC).x, availableWidth - pathW - typeW - 8f); // 8f for margins
            }
            else
            {
                nameW = Mathf.Min(EditorStyles.label.CalcSize(assetNameGC).x, availableWidth - typeW - 4f); // 4f for margin
            }
            
            Rect pathRect = drawFullPath && pathW > 0 ? GUI2.LeftRect(pathW, ref r) : new Rect();
            Rect nameRect = GUI2.LeftRect(nameW, ref r);
            Rect typeRect = GUI2.LeftRect(typeW + 4f, ref r);
            
            pingRect.width = 16f + margin + pathW + nameW + typeW + 4f;
            
            DrawPingRect(pingRect);
            DrawTargetIcon(iconRect, icon, iconTooltip);
            DrawScenePath(pathRect, drawFullPath);
            DrawTargetName(nameRect, assetNameGC);
            DrawTargetType(typeRect, assetTypeGC);

#if UNITY_2022_3_OR_NEWER
            // Draw P only on hover and repaint on mouse move for responsiveness
            Rect rowRect = new Rect(r.x, r.y, r.width, FR2_Theme.Current.TreeItemHeight);
            bool isHover = rowRect.Contains(Event.current.mousePosition);
            if (Event.current.type == EventType.MouseMove)
            {
                var focused = EditorWindow.focusedWindow;
                if (focused != null) focused.Repaint();
            }
            
            var propRect = new Rect(r.xMax - 18f, r.y, 18f, r.height);
            r.width -= 18f;
            
            if (isHover)
            {
                propRect.yMax -= 2f;
                if (GUI.Button(propRect, FR2_GUIContent.FromString("P", "Open Properties"), EditorStyles.miniButton))
                {
                    UnityObject targetForProperties = GetSourceComponentForProperties();
                    if (targetForProperties != null) EditorUtility.OpenPropertyEditor(targetForProperties);
                }
            }
#endif

            DrawReferenceIcons(r);
        }

        (Texture icon, string tooltip) GetTargetIcon()
        {
            if (component == null) return (null, "");
            
            if (component is GameObject go)
                return (EditorGUIUtility.ObjectContent(go, typeof(GameObject)).image, "GameObject");
            if (component is Component comp)
            {
                var compType = comp.GetType();
                return (EditorGUIUtility.ObjectContent(comp, compType).image, GetCachedTypeName(compType));
            }
            
            var componentType = component.GetType();
            return (EditorGUIUtility.ObjectContent(component, componentType).image, GetCachedTypeName(componentType));
        }

        UnityObject GetSourceComponentForProperties()
        {
            if (sourceRefs?.Count > 0)
            {
                return sourceRefs[0].sourceComponent;
            }
            if (backwardRefs?.Count > 0)
            {
                return backwardRefs[0].sourceComponent;
            }
            return component;
        }

        void DrawTargetIcon(Rect iconRect, Texture icon, string iconTooltip)
        {
            if (icon != null)
            {
                var iconContent = FR2_GUIContent.FromTexture(icon, iconTooltip);
                // Ensure icon is drawn with original colors by temporarily resetting GUI.color
                Color originalColor = GUI.color;
                GUI.color = Color.white; // Reset to white to avoid color distortion
                GUI.Label(iconRect, iconContent, EditorStyles.label);
                GUI.color = originalColor; // Restore original color
            }
        }

        void DrawScenePath(Rect pathRect, bool drawFullPath)
        {
            if (!drawFullPath || string.IsNullOrEmpty(scenePath)) return;
            
            using (FR2_GUI.Color(FR2_Theme.Current.SecondaryTextColor))
            {
                ClippedLabel.Draw(pathRect, scenePath + "/", EditorStyles.miniLabel);
            }
        }

        void DrawTargetName(Rect nameRect, GUIContent displayContent)
        {
            if (isSelected())
            {
                using (FR2_GUI.Color(FR2_Theme.Current.SelectionHighlight))
                {
                    GUI.DrawTexture(nameRect, EditorGUIUtility.whiteTexture);
                }
            }
            ClippedLabel.Draw(nameRect, displayContent.text, EditorStyles.label);
        }

        void DrawTargetType(Rect typeRect, GUIContent typeContent)
        {
            if (!string.IsNullOrEmpty(typeContent.text))
            {
                using (FR2_GUI.Color(FR2_Theme.Current.SecondaryTextColor))
                {
                    GUI.Label(typeRect, typeContent, EditorStyles.miniLabel);
                }
            }
        }

        float CalculateReferenceIconsWidth()
        {
            var refInfos = sourceRefs?.Count > 0 ? sourceRefs : backwardRefs;
            if (refInfos == null || refInfos.Count == 0) return 0f;
            
            float width = 18f;
            float totalWidth = width * refInfos.Count;
            bool showDetails = refInfos.Count == 1;
            
            // For multiple references, add space for count display
            if (!showDetails && refInfos.Count > 1)
            {
                var countText = $"({refInfos.Count})";
                var countContent = FR2_GUIContent.FromString(countText);
                var countWidth = EditorStyles.miniLabel.CalcSize(countContent).x;
                totalWidth += countWidth + 4f; // Add space for count text
            }
            
            if (showDetails && refInfos.Count > 0)
            {
                var refInfo = refInfos[0];
                var beautifiedPath = GetCachedBeautifiedPath(refInfo.propertyPath);
                var pathContent = FR2_GUIContent.FromString(beautifiedPath);
                var labelWidth = EditorStyles.miniLabel.CalcSize(pathContent).x;
                var className = GetCachedTypeName(refInfo.sourceComponent.GetType());
                var classNameContent = FR2_GUIContent.FromString(className);
                var classNameWidth = EditorStyles.label.CalcSize(classNameContent).x;
                totalWidth += labelWidth + 4f + classNameWidth;
            }
            
            return totalWidth;
        }

        void DrawReferenceIcons(Rect r)
        {
            if (sourceRefs?.Count > 0)
            {
                DrawReferenceInformation(r, sourceRefs);
            }
            else if (backwardRefs?.Count > 0)
            {
                DrawReferenceInformation(r, backwardRefs);
            }
        }

        void DrawReferenceInformation(Rect r, List<SceneRefInfo> refInfos)
        {
            if (refInfos == null || refInfos.Count == 0) return;
            
            float width = 18f;
            float totalWidth = width * refInfos.Count;
            bool showDetails = refInfos.Count == 1;
            
            // For multiple references, add space for count display
            if (!showDetails && refInfos.Count > 1)
            {
                var countText = $"({refInfos.Count})";
                var countContent = FR2_GUIContent.FromString(countText);
                var countWidth = EditorStyles.miniLabel.CalcSize(countContent).x;
                totalWidth += countWidth + 4f; // Add space for count text
            }
            
            if (showDetails)
            {
                var refInfo = refInfos[0];
                var beautifiedPath = GetCachedBeautifiedPath(refInfo.propertyPath);
                var pathContent = FR2_GUIContent.FromString(beautifiedPath);
                var labelWidth = EditorStyles.miniLabel.CalcSize(pathContent).x;
                var className = GetCachedTypeName(refInfo.sourceComponent.GetType());
                var classNameContent = FR2_GUIContent.FromString(className);
                var classNameWidth = EditorStyles.label.CalcSize(classNameContent).x;
                totalWidth += labelWidth + 4f + classNameWidth;
            }
            
            float startX = r.x + r.width - totalWidth;
            float currentX = startX;
            
            // Draw count for multiple references
            if (!showDetails && refInfos.Count > 1)
            {
                var countText = $"({refInfos.Count})";
                var countContent = FR2_GUIContent.FromString(countText);
                var countWidth = EditorStyles.miniLabel.CalcSize(countContent).x;
                var countRect = new Rect(currentX, r.y, countWidth, r.height);
                
                using (FR2_GUI.Color(FR2_Theme.Current.SecondaryTextColor))
                {
                    GUI.Label(countRect, countContent, EditorStyles.miniLabel);
                }
                
                currentX += countWidth + 4f;
            }
            
            for (int i = 0; i < refInfos.Count; i++)
            {
                var refInfo = refInfos[i];
                if (refInfo.sourceComponent == null) continue;
                
                if (showDetails)
                {
                    // Draw property path
                    var beautifiedPath = GetCachedBeautifiedPath(refInfo.propertyPath);
                    var pathContent = FR2_GUIContent.FromString(beautifiedPath);
                    var labelWidth = EditorStyles.miniLabel.CalcSize(pathContent).x;
                    var labelRect = new Rect(currentX, r.y, labelWidth, r.height);
                    
                    using (FR2_GUI.Color(FR2_Theme.Current.SecondaryTextColor))
                    {
                        GUI.Label(labelRect, pathContent, EditorStyles.miniLabel);
                    }
                    
                    if (GUI.Button(labelRect, GUIContent.none, EditorStyles.label))
                    {
                        EditorGUIUtility.PingObject(refInfo.sourceComponent);
                        FR2_Unity.PingAndHighlight(refInfo.sourceComponent, refInfo.propertyPath); 
                        GUIUtility.ExitGUI();
                    }
                    
                    currentX += labelWidth + 4f;
                }
                
                // Draw component icon with original colors (no color distortion)
                var iconRect = new Rect(currentX, r.y, width, r.height);
                var componentType = refInfo.sourceComponent.GetType();
                var icon = EditorGUIUtility.ObjectContent(refInfo.sourceComponent, componentType).image;
                var tooltipText = refInfo.IsBackwardRef 
                    ? refInfo.propertyPath 
                    : $"{GetCachedTypeName(componentType)}.{refInfo.propertyPath}";
                var content = FR2_GUIContent.FromTexture(icon, tooltipText);
                
                // Ensure icon is drawn with original colors by temporarily resetting GUI.color
                Color originalColor = GUI.color;
                GUI.color = Color.white; // Reset to white to avoid color distortion
                GUI.Label(iconRect, content);
                GUI.color = originalColor; // Restore original color
                
                if (GUI.Button(iconRect, GUIContent.none, EditorStyles.label))
                {
                    EditorGUIUtility.PingObject(refInfo.sourceComponent);
                    FR2_Unity.PingAndHighlight(refInfo.sourceComponent, refInfo.propertyPath); 
                    GUIUtility.ExitGUI();
                }
                
                if (showDetails)
                {
                    currentX += width;
                    
                    // Draw component class name
                    var className = GetCachedTypeName(refInfo.sourceComponent.GetType());
                    var classNameContent = FR2_GUIContent.FromString(className);
                    var classNameWidth = EditorStyles.label.CalcSize(classNameContent).x;
                    var classNameRect = new Rect(currentX, r.y, classNameWidth, r.height);
                    
                    GUI.Label(classNameRect, classNameContent, EditorStyles.label);
                    
                    if (GUI.Button(classNameRect, GUIContent.none, EditorStyles.label))
                    {
                        EditorGUIUtility.PingObject(refInfo.sourceComponent);
                        FR2_Unity.PingAndHighlight(refInfo.sourceComponent, refInfo.propertyPath); 
                        GUIUtility.ExitGUI();
                    }
                }
                else
                {
                    // For multiple icons, just advance by icon width
                    currentX += width;
                }
            }
        }


        static string GetCachedBeautifiedPath(string propertyPath)
        {
            if (string.IsNullOrEmpty(propertyPath)) return "";
            if (beautifiedPathCache.TryGetValue(propertyPath, out string cachedPath)) return cachedPath;
            
            string result = propertyPath;
            result = result.Replace(".Array.data[", "[");
            result = result.Replace("].Array.data[", "][");
            
            beautifiedPathCache.Add(propertyPath, result);
            return result;
        }

        void DrawPingRect(Rect pingRect)
        {
            // Bigger Rect
            pingRect.yMin -= 1;
            pingRect.yMax += 1;

            if (Event.current.type != EventType.MouseDown) return;
            if (Event.current.button != 0) return;
            if (!pingRect.Contains(Event.current.mousePosition)) return;
            
            EditorGUIUtility.PingObject((component as Component).gameObject);
            Event.current.Use();
        }

        public static Dictionary<string, FR2_Ref> FindSceneUseSceneObjects(GameObject[] targets)
        {
            return FR2_SceneCache.FindSceneUseSceneObjects(targets);
        }

        public static Dictionary<string, FR2_Ref> FindSceneBackwardReferences(GameObject[] targets)
        {
            return FR2_SceneCache.FindSceneBackwardReferences(targets);
        }

        public static Dictionary<string, FR2_Ref> FindSceneInScene(GameObject[] targets)
        {
            return FR2_SceneCache.FindSceneInScene(targets);
        }

        public static Dictionary<string, FR2_Ref> FindRefInScene(
            string[] assetGUIDs, bool depth,
            Action<Dictionary<string, FR2_Ref>> onComplete)
        {
            cacheAssetGuids = assetGUIDs;
            onFindRefInSceneComplete = onComplete;
            if (FR2_SceneCache.isReady)
            {
                FindRefInScene();
            } else
            {
                FR2_SceneCache.onReady -= FindRefInScene;
                FR2_SceneCache.onReady += FindRefInScene;
            }

            return refs;
        }

        private static void FindRefInScene()
        {
            if (refs == null) refs = new Dictionary<string, FR2_Ref>();
            else refs.Clear(); // Reuse existing dictionary
            
            for (var i = 0; i < cacheAssetGuids.Length; i++)
            {
                FR2_Asset asset = FR2_Cache.Api.Get(cacheAssetGuids[i]);
                if (asset == null) continue;

                Add(refs, asset, 0);
                ApplyFilter(refs, asset);
            }

            if (onFindRefInSceneComplete != null) onFindRefInSceneComplete(refs);
            FR2_SceneCache.onReady -= FindRefInScene;
        }

        private static void ApplyFilter(Dictionary<string, FR2_Ref> refs, FR2_Asset asset)
        {
            string targetPath = asset.guid.ToAssetPath();
            if (string.IsNullOrEmpty(targetPath)) return;

            if (targetPath != asset.assetPath) asset.MarkAsDirty();

            UnityObject target = AssetDatabase.LoadAssetAtPath(targetPath, typeof(UnityObject));
            if (target == null) return;

            if (target is GameObject)
            {
                foreach (GameObject item in FR2_ObjectExtensions.GetAllGameObjectsInScenes())
                {
                    if (FR2_Unity.CheckIsPrefab(item))
                    {
                        string itemGUID = FR2_Unity.GetPrefabParent(item);
                        if (itemGUID == asset.guid) Add(refs, item, 1);
                    }
                }
            }

            // Search through all cached components for references to this asset
            foreach (var cacheEntry in FR2_SceneCache.Api.cache)
            {
                foreach (var hashValue in cacheEntry.Value)
                {
                    if (targetPath == AssetDatabase.GetAssetPath(hashValue.target))
                    {
                        Add(refs, cacheEntry.Key, 1);
                        break;
                    }
                }
            }
        }

        private static void Add(Dictionary<string, FR2_Ref> refs, FR2_Asset asset, int depth)
        {
            string targetId = asset.guid;
            if (!refs.ContainsKey(targetId)) refs.Add(targetId, new FR2_Ref(0, depth, asset, null));
        }

        private static void Add(Dictionary<string, FR2_Ref> refs, UnityObject target, int depth)
        {
            if (target == null) return;
            var targetId = target.GetInstanceID().ToString();
            if (!refs.ContainsKey(targetId)) refs.Add(targetId, new FR2_SceneRef(depth, target));
        }
    }
    

    

}

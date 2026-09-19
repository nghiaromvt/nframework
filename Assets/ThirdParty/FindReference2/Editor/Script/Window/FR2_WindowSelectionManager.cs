using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace vietlabs.fr2
{
    /// <summary>
    /// Extracted selection management logic from FR2_WindowAll god class.
    /// Handles Unity selection synchronization, caching, and change detection.
    /// </summary>
    internal class FR2_WindowSelectionManager
    {
        private readonly FR2_WindowAll _window;
        private UnityObject[] _cachedSelection;
        private int _cachedSelectionFrame = -1;
        
        public FR2_WindowSelectionManager(FR2_WindowAll window)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
        }
        
        /// <summary>
        /// Gets cached Unity selection, refreshing if needed
        /// </summary>
        public UnityObject[] GetCachedSelection()
        {
            int currentFrame = Time.frameCount;
            if (_cachedSelectionFrame != currentFrame)
            {
                _cachedSelection = Selection.objects;
                _cachedSelectionFrame = currentFrame;
            }
            return _cachedSelection;
        }
        
        /// <summary>
        /// Handles selection manager change events
        /// </summary>
        public void OnSelectionManagerChanged()
        {
            if (_window.selection == null) return;
            if (_window.selection.isLock) return;
            
            // Check and consume ping lock state - if active, skip sync but hide warnings
            bool hadPingLock = _window.smartLock?.ConsumePingLockState() ?? false;
            if (hadPingLock)
            {
                FR2_LOG.Log("Skipped refresh: Ping lock was active - keeping current FR2 selection!");
                
                // Still need to check if selection is out of sync for UI highlighting
                var unitySelection = FR2_SelectionManager.Instance.GetUnitySelection();
                var fr2Selection = _window.selection.GetUnityObjects();
                _window.isSelectionOutOfSync = !AreSelectionsEqual(unitySelection, fr2Selection);
                
                _window.WillRepaint = true;
                _window.Repaint();
                return;
            }
            
            // IMPORTANT: Always update FR2 selection regardless of cache readiness
            // Selection listening must work independently of cache status
            
            var shouldRefresh = _window.smartLock.ShouldRefreshWithSmartLogic(_window, _window.selection.GetUnityObjects());
            if (shouldRefresh)
            {
                SyncSelectionFromUnity();
            }
        }
        
        /// <summary>
        /// Synchronizes FR2 selection with Unity selection
        /// </summary>
        public void SyncSelectionFromUnity()
        {
            var unitySelection = FR2_SelectionManager.Instance.GetUnitySelection();
            
            // Update FR2 selection to match Unity
            _window.selection.Clear();
            foreach (var obj in unitySelection)
            {
                if (obj != null)
                {
                    if (EditorUtility.IsPersistent(obj))
                    {
                        var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
                        if (!string.IsNullOrEmpty(guid))
                        {
                            _window.selection.Add(guid);
                        }
                    }
                    else
                    {
                        _window.selection.Add(obj);
                    }
                }
            }
            _window.isSelectionOutOfSync = false;
            
            // Refresh drawers if cache is ready
            if (FR2_Cache.isReady)
            {
                RefreshDrawers();
            }
            
            _window.WillRepaint = true;
            _window.Repaint();
        }
        
        /// <summary>
        /// Forces FR2 selection to sync with Unity selection
        /// </summary>
        public void ForceSyncFromUnity()
        {
            var unitySelection = FR2_SelectionManager.Instance.GetUnitySelection();
            _window.selection.Clear();
            foreach (var obj in unitySelection)
            {
                if (obj != null)
                {
                    if (EditorUtility.IsPersistent(obj))
                    {
                        var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
                        if (!string.IsNullOrEmpty(guid))
                        {
                            _window.selection.Add(guid);
                        }
                    }
                    else
                    {
                        _window.selection.Add(obj);
                    }
                }
            }
            _window.isSelectionOutOfSync = false;
            
            if (FR2_Cache.isReady)
            {
                RefreshDrawers();
            }
            
            _window.WillRepaint = true;
            _window.Repaint();
        }
        
        /// <summary>
        /// Synchronizes Unity selection with FR2 selection
        /// </summary>
        public void SyncSelectionToUnity()
        {
            if (_window.selection == null) return;
            
            var fr2Selection = _window.selection.GetUnityObjects();
            if (fr2Selection != null && fr2Selection.Length > 0)
            {
                Selection.objects = fr2Selection;
                _window.isSelectionOutOfSync = false;
            }
        }
        
        /// <summary>
        /// Checks if two selection arrays are equal
        /// </summary>
        public static bool AreSelectionsEqual(UnityObject[] selection1, UnityObject[] selection2)
        {
            if (selection1 == null && selection2 == null) return true;
            if (selection1 == null || selection2 == null) return false;
            if (selection1.Length != selection2.Length) return false;
            
            return selection1.SequenceEqual(selection2);
        }
        
        /// <summary>
        /// Gets the current FR2 selection as Unity objects
        /// </summary>
        public UnityObject[] GetFR2Selection()
        {
            return _window.selection?.GetUnityObjects() ?? new UnityObject[0];
        }
        
        /// <summary>
        /// Checks if the current selection is out of sync
        /// </summary>
        public bool IsSelectionOutOfSync()
        {
            var unitySelection = FR2_SelectionManager.Instance.GetUnitySelection();
            var fr2Selection = GetFR2Selection();
            return !AreSelectionsEqual(unitySelection, fr2Selection);
        }
        
        /// <summary>
        /// Updates selection sync status for UI highlighting
        /// </summary>
        public void UpdateSelectionSyncStatus()
        {
            _window.isSelectionOutOfSync = IsSelectionOutOfSync();
        }
        
        /// <summary>
        /// Refreshes all drawers based on current selection
        /// </summary>
        private void RefreshDrawers()
        {
            if (!FR2_Cache.isReady) return;
            
            try
            {
                var selection = _window.selection;
                if (selection == null) return;
                
                // Refresh asset-based drawers
                if (selection.isSelectingAsset)
                {
                    string[] guids = selection.guidSet.ToArray();
                    _window.UsesDrawer.Reset(guids, true);
                    _window.UsedByDrawer.Reset(guids, false);
                }
                
                // Refresh scene-based drawers
                if (selection.isSelectingSceneObject)
                {
                    var gameObjects = new List<GameObject>();
                    foreach (var instId in selection.instSet)
                    {
                        if (int.TryParse(instId, out int id))
                        {
                            var obj = EditorUtility.InstanceIDToObject(id) as GameObject;
                            if (obj != null)
                            {
                                gameObjects.Add(obj);
                            }
                        }
                    }
                    _window.SceneToAssetDrawer.Reset(gameObjects.ToArray(), true, true);
                    _window.RefInScene.Reset(gameObjects.ToArray(), false, false);
                    _window.SceneUsesDrawer.Reset(gameObjects.ToArray(), true, false);
                    _window.RefSceneInScene.Reset(gameObjects.ToArray(), false, false);
                }
                
                FR2_LOG.Log($"Refreshed drawers for selection: {selection.Count} items");
            }
            catch (Exception ex)
            {
                FR2_LOG.LogError($"Failed to refresh drawers: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Clears all drawer data
        /// </summary>
        public void ClearDrawers()
        {
            _window.UsesDrawer?.Reset(null, true);
            _window.UsedByDrawer?.Reset(null, false);
            _window.SceneToAssetDrawer?.Reset(null, true);
            _window.RefInScene?.Reset(null, false);
            _window.SceneUsesDrawer?.Reset(null, true);
            _window.RefSceneInScene?.Reset(null, false);
        }
    }
}

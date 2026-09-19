using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace vietlabs.fr2
{
    internal partial class FR2_WindowAll
    {
        private void DrawScenePanel(Rect rect)
        {
            FR2_RefDrawer drawer = isFocusingUses
                ? IsSelectingAssets ? null : SceneUsesDrawer
                : IsSelectingAssets ? RefInScene : RefSceneInScene;
            
            if (drawer == null) return;

            if (!FR2_SceneCache.isReady && FR2_SceneCache.Api.Status == SceneCacheStatus.Scanning)
            {
                DrawSceneCacheProgress(rect);
                rect.yMin += 18f;
            }
            
            if (FR2_SceneCache.hasCache) drawer.Draw(rect);
        }

        private void DrawSceneCacheProgress(Rect rect)
        {
            Rect rr = rect;
            rr.height = 16f;

            if (FR2_SceneCache.Api.Status == SceneCacheStatus.Scanning)
            {
                int cur = FR2_SceneCache.Api.current, total = FR2_SceneCache.Api.total;
                var progress = Mathf.Clamp01(cur * 1f / total);
                var progressText = FR2_SceneCache.Api.Status == SceneCacheStatus.Scanning
                    ? $"Scanning objects: {cur} / {total}"
                    : $"{cur} / {total}";
                EditorGUI.ProgressBar(rr, progress, progressText);

                if (cur >= total)
                {
                    FR2_LOG.LogWarning($"Stuck at scanning? {cur}/{total}");
                }
                WillRepaint = true;
                return;
            }
            
            string statusText;
            switch (FR2_SceneCache.Api.Status)
            {
                case SceneCacheStatus.None:
                    statusText = "Scene cache is not ready!";
                    break;
                case SceneCacheStatus.Changed:
                    statusText = "Scene changed - results might be incompleted";
                    break;
                case SceneCacheStatus.Scanning:
                    statusText = "Preparing to scan scene objects...";
                    break;
                case SceneCacheStatus.Ready:
                    statusText = "Scene cache ready";
                    break;
                default:
                    statusText = "Unknown status";
                    break;
            }
            
            EditorGUI.ProgressBar(rr, 0f, statusText);
        }



        private void DrawAssetPanel(Rect rect)
        {
            FR2_RefDrawer drawer = GetAssetDrawer();
            if (drawer == null) return;
            drawer.Draw(rect);

            if (!drawer.showDetail) return;

            settings.details = true;
            drawer.showDetail = false;
            sp1.SetSplitVisible(2, settings.details);
            Repaint();
        }

        private void DrawGitWarningPanel()
        {
            if (!FR2_SettingExt.isGitProject || FR2_SettingExt.gitIgnoreAdded || FR2_SettingExt.hideGitIgnoreWarning) return;
            
            EditorGUILayout.BeginHorizontal();
            
            // Left side: Warning message
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            EditorGUILayout.HelpBox("You should add **/FR2_Cache.asset* to your .gitignore file to avoid committing cache files.", MessageType.Warning);
            EditorGUILayout.EndVertical();
            
            // Right side: Buttons stacked vertically
            EditorGUILayout.BeginVertical(FR2_Theme.Current.ApplyButtonWidth);

            if (GUILayout.Button("Apply", FR2_Theme.Current.CompactButtonHeight))
            {
                FR2_GitUtil.AddFR2CacheToGitIgnore();
                FR2_SettingExt.gitIgnoreAdded = true;
            }

            if (GUILayout.Button("Ignore", FR2_Theme.Current.CompactButtonHeight))
            {
                FR2_SettingExt.hideGitIgnoreWarning = true;
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawToolsWarningPanel()
        {
            if (FR2_SettingExt.hideToolsWarning) return;
            
            "Tools are POWERFUL & DANGEROUS! Only use if you know what you are doing!!!".DrawWarningPanel(
                FR2_Icon.Warning.image as Texture2D, 
                () => FR2_SettingExt.hideToolsWarning = true
            );
        }


        internal bool DrawButton(Rect rect, ref bool show, GUIContent icon)
        {
            var changed = false;
            bool currentShow = show;
            bool buttonPressed;
            
            using (FR2_GUI.ContentColor(currentShow ? FR2_Theme.Current.IconActiveColor : FR2_Theme.Current.IconColor))
            {
                buttonPressed = rect.DrawToggleButton(currentShow, icon);
            }
            
            if (buttonPressed)
            {
                show = !currentShow;
                EditorUtility.SetDirty(this);
                WillRepaint = true;
                changed = true;
            }
            
            return changed;
        }

        internal void DrawAssetViewSettings()
        {
            bool isDisable = !sp2.splits[1].visible;
            EditorGUI.BeginDisabledGroup(isDisable);
            {
                GUI2.ToolbarToggle(ref FR2_Setting.s.displayAssetBundleName, FR2_Icon.AssetBundle.image, Vector2.zero, "Show / Hide Assetbundle Names");
#if UNITY_2017_1_OR_NEWER
                GUI2.ToolbarToggle(ref FR2_Setting.s.displayAtlasName, FR2_Icon.Atlas.image, Vector2.zero, "Show / Hide Atlas packing tags");
#endif

                if (GUILayout.Button("CSV", EditorStyles.toolbarButton)) OnCSVClickExtension();
            }
            EditorGUI.EndDisabledGroup();
        }

        private FR2_EnumDrawer groupModeED;
        private FR2_EnumDrawer toolModeED;
        private FR2_EnumDrawer sortModeED;

        internal void DrawViewModes(Rect rect)
        {
            float halfWidth = rect.width * 0.5f;
            Rect rect1 = new Rect(rect.x, rect.y, halfWidth, rect.height);
            Rect rect2 = new Rect(rect.x + halfWidth, rect.y, halfWidth, rect.height);
            
            if (toolModeED == null)
            {
                toolModeED = new FR2_EnumDrawer
                {
                    fr2_enum = new FR2_EnumDrawer.EnumInfo(
                        FR2_RefDrawer.Mode.Type,
                        FR2_RefDrawer.Mode.Folder,
                        FR2_RefDrawer.Mode.Extension
                    )
                };
            }
            if (groupModeED == null) groupModeED = new FR2_EnumDrawer { tooltip = "Group By" };
            if (sortModeED == null) sortModeED = new FR2_EnumDrawer { tooltip = "Sort By" };

            if (settings.toolMode)
            {
                FR2_RefDrawer.Mode tMode = settings.toolGroupMode;
                if (toolModeED.Draw(rect1, ref tMode))
                {
                    settings.toolGroupMode = tMode;
                    MarkDirty();
                    RefreshSort();
                }
            } else
            {
                FR2_RefDrawer.Mode gMode = settings.groupMode;
                if (groupModeED.Draw(rect1, ref gMode))
                {
                    settings.groupMode = gMode;
                    MarkDirty();
                    RefreshSort();
                }
            }

            FR2_RefDrawer.Sort sMode = settings.sortMode;
            if (sortModeED.Draw(rect2, ref sMode))
            {
                settings.sortMode = sMode;
                RefreshSort();
            }
        }
    }
} 
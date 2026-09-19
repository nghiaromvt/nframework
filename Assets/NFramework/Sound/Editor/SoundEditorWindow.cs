using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using NFramework.Sound;

namespace NFramework.Sound.Editor
{
    public class SoundEditorWindow : OdinMenuEditorWindow
    {
        [MenuItem("Tools/NFramework/Sound/Config")]
        public static void OpenConfig()
        {
            var config = SoundConfigSO.GetConfig();
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }

        [MenuItem("Tools/NFramework/Sound/Window")]
        private static void ShowWindow()
        {
            var window = GetWindow<SoundEditorWindow>();
            window.Show();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree(false, new OdinMenuTreeDrawingConfig
            {
                DrawSearchToolbar = true
            });

            tree.Add("Create New Sound Group", new SoundGroupCreator());

            var soundGroups = FileHelper.LoadAssetsWithType<SoundGroupSO>();
            foreach (var group in soundGroups)
            {
                tree.Add($"Sound Groups/{group.name}", group);
            }

            return tree;
        }
        
        protected override void OnBeginDrawEditors()
        {
            base.OnBeginDrawEditors();
            SoundGroupSO soundGroup = MenuTree.Selection.SelectedValue as SoundGroupSO;
            
            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                if (SirenixEditorGUI.ToolbarButton("Generate Script Define"))
                {
                    SoundScriptDefineEditor.GenerateScriptDefine();
                }
                if (SirenixEditorGUI.ToolbarButton("Locate Script Define"))
                {
                    SoundScriptDefineEditor.LocateScriptDefine();
                }

                if (soundGroup)
                {
                    GUILayout.FlexibleSpace();
                    if (SirenixEditorGUI.ToolbarButton("Locate"))
                    {
                        EditorGUIUtility.PingObject(soundGroup);
                    }
                    if (SirenixEditorGUI.ToolbarButton("Delete"))
                    {
                        var path = AssetDatabase.GetAssetPath(soundGroup);
                        if (EditorUtility.DisplayDialog("Delete this?", path + "\n\nYou cannot undo this action", "Delete", "Cancel"))
                        {
                            AssetDatabase.DeleteAsset(path);
                            AssetDatabase.Refresh();
                        }
                    }
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }
    }
}

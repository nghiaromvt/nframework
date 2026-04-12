using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace NFramework.Editor
{
    public class SoundEditorWindow : OdinMenuEditorWindow
    {
        [MenuItem("NFramework/Sound/Window")]
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
            
            var config = NFrameworkConfigSO.GetConfig();
            if (config == null)
                return tree;
            
            tree.Add("Create New Sound Group", new SoundGroupCreator());
            tree.AddAllAssetsAtPath("Sound Groups", $"Assets/{config.soundGroupFolderPath}", typeof(SoundGroupSO), false, true);
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
                        var path = AssetDatabase.GetAssetPath(soundGroup.GetInstanceID());
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

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
            tree.Add("Create New Sound Group", new SoundGroupCreator());
            tree.Add("Generate All Script Defines", new SoundScriptDefineMenu());
            tree.AddAllAssetsAtPath("Sound Groups", "Assets/", typeof(SoundGroupSO), true, true);
            return tree;
        }
        
        protected override void OnBeginDrawEditors()
        {
            base.OnBeginDrawEditors();
            SoundGroupSO soundGroup = MenuTree.Selection.SelectedValue as SoundGroupSO;
            if(!soundGroup) return;
            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                GUILayout.FlexibleSpace();
                GUILayout.FlexibleSpace();
                if (SirenixEditorGUI.ToolbarButton("Locate"))
                {
                    EditorGUIUtility.PingObject(soundGroup);
                }
                if (SirenixEditorGUI.ToolbarButton("Delete"))
                {
                    var path = AssetDatabase.GetAssetPath(soundGroup.GetInstanceID());
                    if (EditorUtility.DisplayDialog("Delete this sound group and its script define?", path + "\n\nYou cannot undo this action", "Delete", "Cancel"))
                    {
                        AssetDatabase.DeleteAsset(path);
                        AssetDatabase.Refresh();
                    }
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }
    }
}

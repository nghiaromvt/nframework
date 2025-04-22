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
            tree.Add("Generate All Script Defines", new GenerateAllScriptDefinesMenu());
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
                if (SirenixEditorGUI.ToolbarButton("Locate SO"))
                {
                    EditorGUIUtility.PingObject(soundGroup);
                }
                if (SirenixEditorGUI.ToolbarButton("Locate Script Define"))
                {
                    soundGroup.LocateScriptDefine();
                }
                if (SirenixEditorGUI.ToolbarButton("Delete"))
                {
                    soundGroup.Delete();
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }
    }
}

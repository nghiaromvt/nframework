using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace NFramework.Editor
{
    public class UIEditorWindow : OdinMenuEditorWindow
    {
        [MenuItem("NFramework/UI/Window")]
        private static void ShowWindow()
        {
            var window = GetWindow<UIEditorWindow>();
            window.Show();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree(false, new OdinMenuTreeDrawingConfig
            {
                DrawSearchToolbar = true
            });
            
            tree.Add("Script Define", new UIScriptDefineMenu());
            
            var prefabs = FileHelper.LoadAssetsWithType<GameObject>("t:Prefab");
            prefabs.ForEach(pf =>
            {
                if (pf.TryGetComponent<UIView>(out var view))
                    tree.AddObjectAtPath($"Layer {view.UILayer}/{pf.name}", view, true);
            });
            
            return tree;
        }
        
        protected override void OnBeginDrawEditors()
        {
            base.OnBeginDrawEditors();
            UIView view = MenuTree.Selection.SelectedValue as UIView;
            if(!view) return;
            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                GUILayout.FlexibleSpace();
                GUILayout.FlexibleSpace();
                if (SirenixEditorGUI.ToolbarButton("Locate"))
                {
                    EditorGUIUtility.PingObject(view);
                }
                if (SirenixEditorGUI.ToolbarButton("Delete"))
                {
                    var path = AssetDatabase.GetAssetPath(view.GetInstanceID());
                    if (EditorUtility.DisplayDialog("Delete this?", path + "\n\nYou cannot undo this action", "Delete", "Cancel"))
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

using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using NFramework.UI;

namespace NFramework.UI.Editor
{
    public class UIEditorWindow : OdinMenuEditorWindow
    {
        [MenuItem("Tools/NFramework/UI/Window")]
        private static void ShowWindow()
        {
            var window = GetWindow<UIEditorWindow>();
            window.Show();
        }

        [MenuItem("Tools/NFramework/UI/Config")]
        public static void OpenConfig()
        {
            var config = UIConfigSO.GetConfig();
            if (config != null)
            {
                Selection.activeObject = config;
                EditorGUIUtility.PingObject(config);
            }
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree(false, new OdinMenuTreeDrawingConfig
            {
                DrawSearchToolbar = true
            });

            var config = UIConfigSO.GetConfig();
            if (config == null)
                return tree;

            tree.Add("Create New View", new UIViewCreator());
            
            var guids = AssetDatabase.FindAssets("t:Prefab");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var view = AssetDatabase.LoadAssetAtPath<UIView>(path);
                if (view != null)
                {
                    tree.AddObjectAtPath($"Layer {view.UILayer}/{view.name}", view, true);
                }
            }
            
            return tree;
        }
        
        protected override void OnBeginDrawEditors()
        {
            base.OnBeginDrawEditors();
            UIView view = MenuTree.Selection.SelectedValue as UIView;
            
            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                if (SirenixEditorGUI.ToolbarButton("Generate ScriptDefine"))
                {
                    UIScriptDefineEditor.GenerateScriptDefine();
                }
                if (SirenixEditorGUI.ToolbarButton("Locate ScriptDefine"))
                {
                    UIScriptDefineEditor.LocateScriptDefine();
                }

                if (view)
                {
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
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }
    }
}

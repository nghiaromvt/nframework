using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace NFramework.Editor
{
    public class OpenConfigEditor : OdinEditorWindow
    {
        private static NFrameworkConfigSO _config;
        
        [MenuItem("NFramework/Open Config")]
        private static void Open()
        {
            _config = NFrameworkConfigSO.GetConfig();
            var window = GetWindow<OpenConfigEditor>();
            InspectObject(window, _config);
        }
        
        protected override void OnBeginDrawEditors()
        {
            base.OnBeginDrawEditors();
            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                GUILayout.FlexibleSpace();
                GUILayout.FlexibleSpace();
                if (SirenixEditorGUI.ToolbarButton("Ping"))
                {
                    EditorGUIUtility.PingObject(_config);
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }
    }
}
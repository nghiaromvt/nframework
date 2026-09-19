using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Rowlan.Tools.QuickNav
{
    public class QuickNavPopupWindow : PopupWindowContent
    {
        private GUIContent toggleEditModeContent = new GUIContent("Toggle Edit Mode", "Toggle edit mode for object name override in Favorites mode. The text will be displayed instead of the object name. Set text to empty to revert back to object name display as default");
        private GUIContent addSeparatorContent = new GUIContent("Add Separator");
        private GUIContent closePopupContent = new GUIContent("Close");

        private QuickNavEditorModule module;
        private string headerText;

        public QuickNavPopupWindow(QuickNavEditorModule module, string headerText)
        {
            this.module = module;
            this.headerText = headerText;
        }
        public override Vector2 GetWindowSize()
        {
            return new Vector2(200, 90);
        }

        public override void OnGUI(Rect rect)
        {
            EditorGUILayout.BeginVertical( GUIStyles.AppTitleBoxStyle);
            {
                EditorGUILayout.LabelField(headerText, GUIStyles.PopupTitleStyle);

                if (GUILayout.Button(toggleEditModeContent))
                {
                    module.GetDataManager().ToggleEditMode();
                    editorWindow.Close();
                }

                if (GUILayout.Button(addSeparatorContent))
                {
                    module.GetDataManager().AddSeparatorToFavorites();
                    editorWindow.Close();
                }

                //GUILayout.FlexibleSpace();

                if (GUILayout.Button(closePopupContent))
                {
                    editorWindow.Close();
                }
            }
            EditorGUILayout.EndVertical();

        }

        public override void OnOpen()
        {
        }

        public override void OnClose()
        {
        }
    }
}
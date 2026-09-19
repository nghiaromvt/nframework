using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using static Rowlan.Tools.QuickNav.QuickNavEditorModule;

namespace Rowlan.Tools.QuickNav
{
    public class QuickNavListControl : ReorderableList
    {
        private int currentSelectionIndex = 0;

        private QuickNavEditorModule module;
        private ModuleType moduleType;

        #region Layout

        float margin = 3;

        // float objectIconWidth = 16;
        // float pingButtonWidth = 30;
        float jumpButtonWidth = 30;
        float favoriteButtonWidth = 30;
        float deleteButtonWidth = 30;
        float contextButtonWidth = 28;

        #endregion Layout

        public QuickNavListControl(QuickNavEditorModule module, string headerText, bool reorderEnabled, bool headerVisible, UnityEditor.SerializedObject serializedObject, UnityEditor.SerializedProperty serializedProperty) : base(serializedObject, serializedProperty, reorderEnabled, headerVisible, false, false)
        {
            this.module = module;
            this.moduleType = module.GetModuleType();

            // list header
            drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, headerText);
            };

            drawElementCallback = (rect, index, active, focused) =>
            {
                // Get the currently to be drawn element from the list
                SerializedProperty element = serializedProperty.GetArrayElementAtIndex(index);

                var contextProperty = element.FindPropertyRelative("context");

                // get the context and the object
                bool isSeparatorContext = contextProperty.enumValueIndex == (int)QuickNavItem.Context.Separator;

                if( isSeparatorContext)
                {
                    DrawSeparatorItemContext(rect, serializedObject, element);
                }
                else
                {
                    DrawQuickNavItemContext(rect, serializedObject, element);
                }

                // advance to next line for the next property
                rect.y += EditorGUIUtility.singleLineHeight;
            };

            /*
            elementHeightCallback = (index) =>
            {
                return EditorGUIUtility.singleLineHeight;
            };
            */
        }

        private void DrawSeparatorItemContext(Rect rect, SerializedObject serializedObject, SerializedProperty element)
        {
            var separatorNameProperty = element.FindPropertyRelative("title");

            float left = 0;
            float width = 0;
            float right = 0;

            width = EditorGUIUtility.currentViewWidth - (right + margin) - deleteButtonWidth - margin * 6 - 23;
            left = right + margin; right = left + width;

            separatorNameProperty.stringValue = EditorGUI.TextField(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), separatorNameProperty.stringValue, GUIStyles.SeparatorStyle);

            // delete button
            {
                width = deleteButtonWidth;
                left = right + margin; right = left + width;
                if (GUI.Button(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), GUIStyles.DeleteIcon))
                {
                    module.GetQuickNavItemList().RemoveAt(index);

                    EditorUtility.SetDirty(serializedObject.targetObject);
                }
            }
        }

        private void DrawQuickNavItemContext( Rect rect, SerializedObject serializedObject, SerializedProperty element)
        {
            var contextProperty = element.FindPropertyRelative("context");
            var titleProperty = element.FindPropertyRelative("title");

            bool isProjectContext = contextProperty.enumValueIndex == (int)QuickNavItem.Context.Project;

            var unityObjectProperty = element.FindPropertyRelative("unityObject");
            var objectGuidProperty = element.FindPropertyRelative("objectGuid");

            float left = 0;
            float width = 0;
            float right = 0;

            UnityEngine.Object currentObject = unityObjectProperty.objectReferenceValue;

            // context: scene icon or project open file button
            {
                width = contextButtonWidth;
                left = right + margin; right = left + width;

                // project: open file button
                if (isProjectContext)
                {
                    GUIContent guiContent = GUIStyles.ProjectIcon;
                    guiContent.text = null;

                    if (GUI.Button(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), GUIStyles.OpenIcon))
                    {
                        // get item
                        QuickNavItem navItem = new QuickNavItem(currentObject, isProjectContext);

                        // get instance id
                        int instanceId = navItem.unityObject.GetInstanceID();

                        // open file
                        if (AssetDatabase.Contains(instanceId))
                        {
                            AssetDatabase.OpenAsset(instanceId);
                        }
                        else
                        {
                            Debug.LogError("Instance not found for object " + navItem.unityObject);
                        }
                    }

                }
                // scene: hierarchy icon
                else
                {
                    // create guicontent, but remove the text; we only want the icon
                    GUIContent guiContent = GUIStyles.SceneIcon;
                    guiContent.text = null;

                    // show icon
                    EditorGUI.LabelField(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), guiContent, GUIStyles.ImageCenterStyle);

                }
            }

            // jump button
            {
                width = jumpButtonWidth;
                left = right + margin; right = left + width;
                if (GUI.Button(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), GUIStyles.JumpIcon))
                {
                    currentSelectionIndex = index;
                    module.JumpToQuickNavItem(false, currentSelectionIndex);
                }
            }

            // object icon
            /*
            {
                width = objectIconWidth;
                left = right + margin; right = left + width;


                // create guicontent, but remove the text; we only want the icon
                GUIContent gc = EditorGUIUtility.ObjectContent(currentObject, typeof(object));
                gc.text = null;

                // show icon
                EditorGUI.LabelField(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), gc);
            }
            */

            // object name
            string displayName = titleProperty.stringValue;
            {
                //width = 128;
                width = EditorGUIUtility.currentViewWidth - (right + margin) - jumpButtonWidth - margin * 3 - 22;
                width -= moduleType == ModuleType.History ? favoriteButtonWidth : 10; // favorites button isn't shown in favorites tab; however there's a drag handle and we don't want the delete button cut off when the scrollbars appear => use arbitrary value (need to find out scrollbar width later)
                left = right + margin; right = left + width;

                // get the display name; by default it's the title (the override)
                {
                    // if there's no explicit title use the object name
                    if (string.IsNullOrEmpty(displayName))
                    {
                        if (unityObjectProperty.objectReferenceValue != null)
                        {
                            displayName = unityObjectProperty.objectReferenceValue.name;
                        }
                    }

                    // in any case if the object is invalid, set the text to "<invalid>"
                    if (unityObjectProperty.objectReferenceValue == null)
                    {
                        displayName = "<invalid>";
                    }
                }

                // depending on the edit mode we allow changing of the object name
                if (module.GetDataManager().IsEditModeEnabled())
                {
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        displayName = EditorGUI.TextField(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), displayName);

                        if (check.changed)
                        {
                            titleProperty.stringValue = displayName;
                        }
                    }
                }
                // view mode: show button
                else
                {
                    GUIContent objectContent = EditorGUIUtility.ObjectContent(currentObject, typeof(object));
                    if (!string.IsNullOrEmpty(displayName))
                    {
                        objectContent.text = displayName;
                    }

                    if (GUI.Button(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), objectContent, GUIStyles.ObjectButtonStyle))
                    {
                        currentSelectionIndex = index;
                        module.JumpToQuickNavItem(true, currentSelectionIndex);
                    }
                }
            }

            /*
            // object property
            {
                // textfield is stretched => calculate it from total length - left position - all the buttons to the right - number of margins ... and the fixed number is just arbitrary
                width = EditorGUIUtility.currentViewWidth - (right + margin) - jumpButtonWidth - favoriteButtonWidth - margin * 3 - 22;
                left = right + margin; right = left + width;

                EditorGUI.PropertyField(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), unityObjectProperty, GUIContent.none);
            }
            */

            // favorite button
            if (moduleType == ModuleType.History)
            {
                bool isFavoritesItem = module.GetDataManager().IsFavoritesItem(currentObject);

                bool guiEnabledPrev = GUI.enabled;
                {
                    // disable the button in case it is already a favorite
                    GUI.enabled = !isFavoritesItem;

                    width = favoriteButtonWidth;
                    left = right + margin; right = left + width;
                    if (GUI.Button(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), GUIStyles.FavoriteIcon))
                    {

                        QuickNavItem navItem = new QuickNavItem(currentObject, isProjectContext);

                        module.GetDataManager().AddToFavorites(navItem);

                        EditorUtility.SetDirty(serializedObject.targetObject);

                    }
                }
                GUI.enabled = guiEnabledPrev;
            }

            // delete button
            {
                width = deleteButtonWidth;
                left = right + margin; right = left + width;
                if (GUI.Button(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), GUIStyles.DeleteIcon))
                {
                    bool performDelete = true;

                    if (QuickNavSettingsProvider.ConfirmDelete)
                    {
                        performDelete = EditorUtility.DisplayDialog("Confirmation", $"Delete '{displayName}'?", "Yes", "No");
                    }

                    if (performDelete)
                    {
                        module.GetQuickNavItemList().RemoveAt(index);

                        EditorUtility.SetDirty(serializedObject.targetObject);
                    }

                }
            }


            // instance id; not relevant to show for now
            /*
            width = 50;
            left = right + margin; right = left + width;
            EditorGUI.BeginDisabledGroup(true);
            {
                EditorGUI.PropertyField(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), instanceIdProperty, GUIContent.none);
            }
            EditorGUI.EndDisabledGroup();
            */
        }

        public int Next()
        {
            currentSelectionIndex++;
            if (currentSelectionIndex >= module.GetItemCount() - 1)
                currentSelectionIndex = module.GetItemCount() - 1;

            if (currentSelectionIndex < 0)
                currentSelectionIndex = 0;

            return currentSelectionIndex;
        }

        public int Previous()
        {
            currentSelectionIndex--;
            if (currentSelectionIndex < 0)
                currentSelectionIndex = 0; 

            return currentSelectionIndex;
        }

        public void Reset()
        {
            currentSelectionIndex = 0;
        }

        public int GetCurrentSelectionIndex()
        {
            return currentSelectionIndex;
        }
    }
}
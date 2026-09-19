using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Rowlan.Tools.QuickNav
{
    public class GUIStyles
    {
        public const float TOOLBAR_BUTTON_HEIGHT = 24;

        private static GUIStyle _appTitleBoxStyle;
        public static GUIStyle AppTitleBoxStyle
        {
            get
            {
                if (_appTitleBoxStyle == null)
                {
                    _appTitleBoxStyle = new GUIStyle("helpBox");
                    _appTitleBoxStyle.fontStyle = FontStyle.Bold;
                    _appTitleBoxStyle.fontSize = 16;
                    _appTitleBoxStyle.alignment = TextAnchor.MiddleCenter;
                }
                return _appTitleBoxStyle;
            }
        }

        private static GUIStyle _boxTitleStyle;
        public static GUIStyle BoxTitleStyle
        {
            get
            {
                if (_boxTitleStyle == null)
                {
                    _boxTitleStyle = new GUIStyle("Label");
                    _boxTitleStyle.fontStyle = FontStyle.BoldAndItalic;
                }
                return _boxTitleStyle;
            }
        }

        private static GUIStyle _popupTitleStyle;
        public static GUIStyle PopupTitleStyle
        {
            get
            {
                if (_popupTitleStyle == null)
                {
                    _popupTitleStyle = new GUIStyle("Label");
                    _popupTitleStyle.fontStyle = FontStyle.BoldAndItalic;
                }
                return _popupTitleStyle;
            }
        }

        private static GUIStyle _separatorStyle;
        public static GUIStyle SeparatorStyle
        {
            get
            {
                if (_separatorStyle == null)
                {
                    _separatorStyle = new GUIStyle(GUI.skin.label);
                    //_separatorStyle.alignment = TextAnchor.MiddleLeft;
                    _separatorStyle.alignment = TextAnchor.MiddleCenter;
                    _separatorStyle.fontStyle = FontStyle.BoldAndItalic;
                    //_separatorStyle.fontSize = 40;
                }
                return _separatorStyle;
            }
        }


        private static GUIStyle _objectButtonStyle;
        public static GUIStyle ObjectButtonStyle
        {
            get
            {
                if (_objectButtonStyle == null)
                {
                    _objectButtonStyle = new GUIStyle(GUI.skin.button);
                    _objectButtonStyle.alignment = TextAnchor.MiddleLeft;
                }
                return _objectButtonStyle;
            }
        }

        private static GUIContent _jumpIcon;
        public static GUIContent JumpIcon
        {
            get
            {
                if (_jumpIcon == null)
                {
                    string iconName = EditorGUIUtility.isProSkin ? "d_search_icon@2x" : "Search Icon";
                    _jumpIcon = EditorGUIUtility.IconContent(iconName, "Jump to Selection");
                    _jumpIcon.tooltip = "Jump to Selection";
                }

                return _jumpIcon;
            }
        }

        private static GUIContent _projectIcon;
        public static GUIContent ProjectIcon
        {
            get
            {
                if (_projectIcon == null)
                {
                    string iconName = EditorGUIUtility.isProSkin ? "d_Project@2x" : "Project";
                    _projectIcon = EditorGUIUtility.IconContent(iconName, "Project");
                    _projectIcon.tooltip = "Project";
                }

                return _projectIcon;
            }
        }

        private static GUIContent _sceneIcon;
        public static GUIContent SceneIcon
        {
            get
            {
                if (_sceneIcon == null)
                {
                    string iconName = EditorGUIUtility.isProSkin ? "d_UnityEditor.SceneHierarchyWindow@2x" : "UnityEditor.SceneHierarchyWindow";
                    _sceneIcon = EditorGUIUtility.IconContent( iconName, "Scene");
                    _sceneIcon.tooltip = "Scene";
                }

                return _sceneIcon;
            }
        }

        private static GUIStyle _imageCenterStyleStyle;
        public static GUIStyle ImageCenterStyle
        {
            get
            {
                if (_imageCenterStyleStyle == null)
                {
                    _imageCenterStyleStyle = new GUIStyle(GUI.skin.label);
                    _imageCenterStyleStyle.alignment = TextAnchor.MiddleCenter;
                }
                return _imageCenterStyleStyle;
            }
        }

        private static GUIContent _addIcon;
        public static GUIContent AddIcon
        {
            get
            {
                if (_addIcon == null)
                {
                    string iconName = EditorGUIUtility.isProSkin ? "d_Toolbar Plus@2x" : "Toolbar Plus";
                    _addIcon = EditorGUIUtility.IconContent(iconName, "Add Selected");
                    _addIcon.tooltip = "Add Selection to Favorites";
                }

                return _addIcon;
            }
        }

        private static GUIContent _clearIcon;
        public static GUIContent ClearIcon
        {
            get
            {
                if (_clearIcon == null)
                {
                    _clearIcon = new GUIContent("Clear");
                    _clearIcon.tooltip = "Remove all items";
                }

                return _clearIcon;
            }
        }

        private static GUIContent _deleteIcon;
        public static GUIContent DeleteIcon
        {
            get
            {
                if (_deleteIcon == null)
                {
                    string iconName = EditorGUIUtility.isProSkin ? "d_TreeEditor.Trash" : "TreeEditor.Trash";
                    _deleteIcon = EditorGUIUtility.IconContent( iconName, "Delete");
                    _deleteIcon.tooltip = "Delete";
                }

                return _deleteIcon;
            }
        }

        private static GUIContent _favoriteIcon;
        public static GUIContent FavoriteIcon
        {
            get
            {
                if (_favoriteIcon == null)
                {
                    string iconName = EditorGUIUtility.isProSkin ? "d_Favorite Icon" : "Favorite Icon";
                    _favoriteIcon = EditorGUIUtility.IconContent(iconName, "Favorite");
                    _favoriteIcon.tooltip = "Add to Favorites";
                }

                return _favoriteIcon;
            }
        }


        private static GUIContent _leftIcon;
        public static GUIContent LeftIcon
        {
            get
            {
                if (_leftIcon == null)
                {
                    string iconName = EditorGUIUtility.isProSkin ? "d_scrollleft_uielements@2x" : "scrollleft_uielements@2x";
                    _leftIcon = EditorGUIUtility.IconContent(iconName, "Previous");
                    _leftIcon.tooltip = "Jump to Previous";
                }

                return _leftIcon;
            }
        }

        private static GUIContent _rightIcon;
        public static GUIContent RightIcon
        {
            get
            {
                if (_rightIcon == null)
                {
                    string iconName = EditorGUIUtility.isProSkin ? "d_scrollright_uielements@2x" : "scrollright_uielements@2x";
                    _rightIcon = EditorGUIUtility.IconContent(iconName, "Next");
                    _rightIcon.tooltip = "Jump to Next";
                }

                return _rightIcon;
            }
        }

        private static GUIContent _downIcon;
        public static GUIContent DownIcon
        {
            get
            {
                if (_downIcon == null)
                {
                    string iconName = EditorGUIUtility.isProSkin ? "d_scrolldown_uielements@2x" : "scrolldown_uielements@2x";
                    _downIcon = EditorGUIUtility.IconContent(iconName, "Previous");
                    _downIcon.tooltip = "Jump to Previous";
                }

                return _downIcon;
            }
        }

        private static GUIContent _upIcon;
        public static GUIContent UpIcon
        {
            get
            {
                if (_upIcon == null)
                {
                    string iconName = EditorGUIUtility.isProSkin ? "d_scrollup_uielements@2x" : "scrollup_uielements@2x";
                    _upIcon = EditorGUIUtility.IconContent(iconName, "Next");
                    _upIcon.tooltip = "Jump to Next";
                }

                return _upIcon;
            }
        }

        private static GUIContent _openIcon;
        public static GUIContent OpenIcon
        {
            get
            {
                if (_openIcon == null)
                {
                    string iconName = EditorGUIUtility.isProSkin ? "FolderOpened Icon" : "FolderOpened Icon";
                    _openIcon = EditorGUIUtility.IconContent(iconName, "Open");
                    _openIcon.tooltip = "Open File";
                }

                return _openIcon;
            }
        }
    }
}
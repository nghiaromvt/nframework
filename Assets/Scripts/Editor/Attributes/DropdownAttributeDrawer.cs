﻿using System;
using UnityEditor;
using UnityEngine;

namespace NFramework.Editors
{
    [CustomPropertyDrawer(typeof(DropdownAttribute))]
    public class DropdownAttributeDrawer : PropertyDrawer
    {
        protected DropdownAttribute _dropdownAttribute;
        protected string[] _dropdownValues;
        protected int _selectedDropdownValueIndex = -1;
        protected Type _propertyType;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_dropdownAttribute == null)
            {
                _dropdownAttribute = (DropdownAttribute)attribute;
                if (_dropdownAttribute.dropdownValues == null || _dropdownAttribute.dropdownValues.Length == 0)
                {
                    return;
                }

                _propertyType = _dropdownAttribute.dropdownValues[0].GetType();

                _dropdownValues = new string[_dropdownAttribute.dropdownValues.Length];
                for (int i = 0; i < _dropdownAttribute.dropdownValues.Length; i++)
                {
                    _dropdownValues[i] = _dropdownAttribute.dropdownValues[i].ToString();
                }

                bool found = false;
                for (var i = 0; i < _dropdownValues.Length; i++)
                {
                    if ((_propertyType == typeof(string)) && property.stringValue == _dropdownValues[i])
                    {
                        _selectedDropdownValueIndex = i;
                        found = true;
                        break;
                    }
                    if ((_propertyType == typeof(int)) && property.intValue == Convert.ToInt32(_dropdownValues[i]))
                    {
                        _selectedDropdownValueIndex = i;
                        found = true;
                        break;
                    }
                    if ((_propertyType == typeof(float)) && Mathf.Approximately(property.floatValue, Convert.ToSingle(_dropdownValues[i])))
                    {
                        _selectedDropdownValueIndex = i;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    _selectedDropdownValueIndex = 0;
                }
            }

            if ((_dropdownValues == null) || (_dropdownValues.Length == 0) || (_selectedDropdownValueIndex < 0))
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            EditorGUI.BeginChangeCheck();
            _selectedDropdownValueIndex = EditorGUI.Popup(position, label.text, _selectedDropdownValueIndex, _dropdownValues);
            if (EditorGUI.EndChangeCheck())
            {
                if (_propertyType == typeof(string))
                {
                    property.stringValue = _dropdownValues[_selectedDropdownValueIndex];
                }
                else if (_propertyType == typeof(int))
                {
                    property.intValue = Convert.ToInt32(_dropdownValues[_selectedDropdownValueIndex]);
                }
                else if (_propertyType == typeof(float))
                {
                    property.floatValue = Convert.ToSingle(_dropdownValues[_selectedDropdownValueIndex]);
                }
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace NFramework.Editors
{
    public class FoldoutAttributeHandler
    {
        private readonly Dictionary<string, CacheFoldProp> _cacheFoldouts = new Dictionary<string, CacheFoldProp>();
        private readonly List<SerializedProperty> _props = new List<SerializedProperty>();
        private bool _initialized;

        private readonly UnityEngine.Object _target;
        private readonly SerializedObject _serializedObject;

        public bool OverrideInspector => _props.Count > 0;

        public FoldoutAttributeHandler(UnityEngine.Object target, SerializedObject serializedObject)
        {
            _target = target;
            _serializedObject = serializedObject;
        }

        public void OnDisable()
        {
            if (_target == null) return;

            foreach (var c in _cacheFoldouts)
            {
                EditorPrefs.SetBool(string.Format($"{c.Value.attribute.name}{c.Value.properties[0].name}{_target.name}"), c.Value.expanded);
                c.Value.Dispose();
            }
        }

        public void Update()
        {
            _serializedObject.Update();
            Setup();
        }

        public void OnInspectorGUI()
        {
            Header();
            Body();

            _serializedObject.ApplyModifiedProperties();
        }

        private void Header()
        {
            using (new EditorGUI.DisabledScope("m_Script" == _props[0].propertyPath))
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_props[0], true);
                EditorGUILayout.Space();
            }
        }

        private void Body()
        {
            foreach (var pair in _cacheFoldouts)
            {
                EditorGUILayout.BeginVertical(StyleFramework.box);
                Foldout(pair.Value);
                EditorGUILayout.EndVertical();

                EditorGUI.indentLevel = 0;
            }

            EditorGUILayout.Space();

            for (var i = 1; i < _props.Count; i++)
            {
                EditorGUILayout.PropertyField(_props[i], true);
            }

            EditorGUILayout.Space();
        }

        private void Foldout(CacheFoldProp cache)
        {
            cache.expanded = EditorGUILayout.Foldout(cache.expanded, cache.attribute.name, true, StyleFramework.foldoutHeader);
            var rect = GUILayoutUtility.GetLastRect();
            rect.x -= 18;
            rect.y -= 4;
            rect.height += 8;
            rect.width += 22;
            EditorGUI.LabelField(rect, GUIContent.none, EditorStyles.helpBox);

            if (cache.expanded)
            {
                EditorGUILayout.Space(2);

                foreach (var property in cache.properties)
                {
                    EditorGUILayout.BeginVertical(StyleFramework.boxChild);
                    EditorGUILayout.PropertyField(property, new GUIContent(ObjectNames.NicifyVariableName(property.name)), true);
                    EditorGUILayout.EndVertical();
                }
            }
        }

        private void Setup()
        {
            if (_initialized) return;

            FoldoutAttribute prevFold = default;

            var length = EditorTypes.Get(_target, out var objectFields);

            for (var i = 0; i < length; i++)
            {
                #region FOLDERS

                var fold = Attribute.GetCustomAttribute(objectFields[i], typeof(FoldoutAttribute)) as FoldoutAttribute;
                CacheFoldProp c;
                if (fold == null)
                {
                    if (prevFold != null && prevFold.foldEverything)
                    {
                        if (!_cacheFoldouts.TryGetValue(prevFold.name, out c))
                        {
                            _cacheFoldouts.Add(prevFold.name,
                                new CacheFoldProp { attribute = prevFold, types = new HashSet<string> { objectFields[i].Name } });
                        }
                        else
                        {
                            c.types.Add(objectFields[i].Name);
                        }
                    }

                    continue;
                }

                prevFold = fold;

                if (!_cacheFoldouts.TryGetValue(fold.name, out c))
                {
                    var expanded = EditorPrefs.GetBool(string.Format($"{fold.name}{objectFields[i].Name}{_target.name}"), false);
                    _cacheFoldouts.Add(fold.name,
                        new CacheFoldProp { attribute = fold, types = new HashSet<string> { objectFields[i].Name }, expanded = expanded });
                }
                else c.types.Add(objectFields[i].Name);

                #endregion
            }

            var property = _serializedObject.GetIterator();
            var next = property.NextVisible(true);
            if (next)
            {
                do
                {
                    HandleFoldProp(property);
                } while (property.NextVisible(false));
            }

            _initialized = true;
        }

        private void HandleFoldProp(SerializedProperty prop)
        {
            bool shouldBeFolded = false;

            foreach (var pair in _cacheFoldouts)
            {
                if (pair.Value.types.Contains(prop.name))
                {
                    var pr = prop.Copy();
                    shouldBeFolded = true;
                    pair.Value.properties.Add(pr);

                    break;
                }
            }

            if (shouldBeFolded == false)
            {
                var pr = prop.Copy();
                _props.Add(pr);
            }
        }

        private class CacheFoldProp
        {
            public HashSet<string> types = new HashSet<string>();
            public readonly List<SerializedProperty> properties = new List<SerializedProperty>();
            public FoldoutAttribute attribute;
            public bool expanded;

            public void Dispose()
            {
                properties.Clear();
                types.Clear();
                attribute = null;
            }
        }
    }

    public static class StyleFramework
    {
        public static readonly GUIStyle box;
        public static readonly GUIStyle boxChild;
        public static readonly GUIStyle foldoutHeader;

        static StyleFramework()
        {
            foldoutHeader = new GUIStyle(EditorStyles.foldout);
            foldoutHeader.overflow = new RectOffset(-10, 0, 3, 0);
            foldoutHeader.padding = new RectOffset(20, 0, 0, 0);
            foldoutHeader.border = new RectOffset(2, 2, 2, 2);

            box = new GUIStyle(GUI.skin.box);
            box.padding = new RectOffset(18, 0, 4, 4);

            boxChild = new GUIStyle(GUI.skin.box);
        }
    }

    public static class EditorTypes
    {
        private static readonly Dictionary<int, List<FieldInfo>> fields = new Dictionary<int, List<FieldInfo>>(FastComparable.DEFAULT);

        public static int Get(System.Object target, out List<FieldInfo> objectFields)
        {
            var t = target.GetType();
            var hash = t.GetHashCode();

            if (!fields.TryGetValue(hash, out objectFields))
            {
                var typeTree = GetTypeTree(t);
                objectFields = target.GetType()
                    .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.NonPublic)
                    .OrderByDescending(x => typeTree.IndexOf(x.DeclaringType))
                    .ToList();
                fields.Add(hash, objectFields);
            }

            return objectFields.Count;
        }

        static IList<Type> GetTypeTree(Type t)
        {
            var types = new List<Type>();
            while (t.BaseType != null)
            {
                types.Add(t);
                t = t.BaseType;
            }

            return types;
        }
    }

    public class FastComparable : IEqualityComparer<int>
    {
        public static readonly FastComparable DEFAULT = new FastComparable();

        public bool Equals(int x, int y)
        {
            return x == y;
        }

        public int GetHashCode(int obj)
        {
            return obj.GetHashCode();
        }
    }
}

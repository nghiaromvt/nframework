using UnityEditor;
using UnityEngine;

namespace NFramework.Editors
{
    [CustomEditor(typeof(Object), true), CanEditMultipleObjects]
    public class UnityObjectEditor : Editor
    {
        private FoldoutAttributeHandler _foldout;
        private ButtonMethodAttributeHandler _buttonMethod;

        private void OnEnable()
        {
            if (target == null) 
                return;

            _foldout = new FoldoutAttributeHandler(target, serializedObject);
            _buttonMethod = new ButtonMethodAttributeHandler(target);
        }

        private void OnDisable() => _foldout?.OnDisable();

        public override void OnInspectorGUI()
        {
            _buttonMethod?.OnBeforeInspectorGUI();

            if (_foldout != null)
            {
                _foldout.Update();
                if (!_foldout.OverrideInspector) 
                    base.OnInspectorGUI();
                else 
                    _foldout.OnInspectorGUI();
            }

            _buttonMethod?.OnAfterInspectorGUI();
        }
    }
}

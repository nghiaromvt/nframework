using System;
using UnityEditor;
using UnityEngine;

namespace NFramework.Editor
{
    public class AesEditorWindow : EditorWindow
    {
        private string _inputText;
        private string _outputText;
        private string _key;
        private string _editorGlobalAesKey;
        private bool _useCustomKey;

        [MenuItem("NFramework/AES Window")]
        private static void Init()
        {
            AesEditorWindow window = (AesEditorWindow)GetWindow(typeof(AesEditorWindow));
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("EditorGlobalAesKey:", EditorStyles.boldLabel);
            _editorGlobalAesKey = AesHelper.GetGlobalEditorAesKey();
            _editorGlobalAesKey = EditorGUILayout.TextField(_editorGlobalAesKey);
            if (GUILayout.Button("Update"))
            {
                EditorPrefs.SetString(AesHelper.EditorGlobalAesPrefs, _editorGlobalAesKey);
                Debug.Log($"{AesHelper.EditorGlobalAesPrefs} => {_editorGlobalAesKey}");
            }

            GUILayout.Space(25);

            GUILayout.Label("Input Text:", EditorStyles.boldLabel);
            _inputText = EditorGUILayout.TextArea(_inputText);

            _useCustomKey = EditorGUILayout.Toggle("Use Custom Key", _useCustomKey);

            if (_useCustomKey)
            {
                GUILayout.Label("Key:", EditorStyles.boldLabel);
                _key = EditorGUILayout.TextField(_key);
            }

            GUILayout.Label("Output Text:", EditorStyles.boldLabel);
            EditorGUILayout.TextArea(_outputText);

            if (GUILayout.Button("Encrypt"))
            {
                try
                {
                    _outputText = AesHelper.EncryptAes(_inputText, _useCustomKey ? _key : AesHelper.GetGlobalEditorAesKey());
                }
                catch (Exception e)
                {
                    Debug.LogError("Encryption failed: " + e.Message);
                }
            }

            if (GUILayout.Button("Decrypt"))
            {
                try
                {
                    _outputText = AesHelper.DecryptAes(_inputText, _useCustomKey ? _key : AesHelper.GetGlobalEditorAesKey());
                }
                catch (Exception e)
                {
                    Debug.LogError("Decryption failed: " + e.Message);
                }
            }
        }
    }
}

using System;
using UnityEditor;
using UnityEngine;

namespace NFramework.Editor
{
    public class AESEditorWindow : EditorWindow
    {
        private string _inputText;
        private string _outputText;
        private string _key;
        private string _editorGlobalAESKey;
        private bool _useCustomKey;

        [MenuItem("NFramework/AES Editor")]
        private static void Init()
        {
            AESEditorWindow window = (AESEditorWindow)GetWindow(typeof(AESEditorWindow));
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("EditorGlobalAESKey:", EditorStyles.boldLabel);
            _editorGlobalAESKey = AESHelper.GetGlobalEditorAESKey();
            _editorGlobalAESKey = EditorGUILayout.TextField(_editorGlobalAESKey);
            if (GUILayout.Button("Update"))
            {
                EditorPrefs.SetString(AESHelper.EDITOR_GLOBAL_AES_KEY_STRING, _editorGlobalAESKey);
                Debug.Log($"{AESHelper.EDITOR_GLOBAL_AES_KEY_STRING} => {_editorGlobalAESKey}");
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
                    _outputText = AESHelper.EncryptAES(_inputText, _useCustomKey ? _key : AESHelper.GetGlobalEditorAESKey());
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
                    _outputText = AESHelper.DecryptAES(_inputText, _useCustomKey ? _key : AESHelper.GetGlobalEditorAESKey());
                }
                catch (Exception e)
                {
                    Debug.LogError("Decryption failed: " + e.Message);
                }
            }
        }
    }
}

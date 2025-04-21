using UnityEditor;

namespace NFramework.Editor
{
    public static class ClearAllEditorPrefs
    {
        [MenuItem("NFramework/Clear All EditorPrefs")]
        public static void Clear()
        {
            if (EditorUtility.DisplayDialog("Warning!", "Do you want to Clear All EditorPrefs?", "OK", "Cancel"))
                EditorPrefs.DeleteAll();
        }
    }
}


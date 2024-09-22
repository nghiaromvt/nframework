using UnityEditor;

namespace NFramework.Editors
{
    public static class ClearAllEditorPrefs
    {
        [MenuItem("NFramework/Clear All EditorPrefs")]
        public static void Clear() => EditorPrefs.DeleteAll();
    }
}


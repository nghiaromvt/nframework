using UnityEditor;

namespace NFramework.Editor
{
    public class DeleteLocalSaveMenuItem
    {
        [MenuItem("NFramework/Delete Local Save")]
        public static void DeleteSave()
        {
            if (!EditorUtility.DisplayDialog("Warning!", "Do you want to Delete Local Save?", "OK", "Cancel"))
                return;

            LocalSaveManager.DeleteSave();
        }
    }
}

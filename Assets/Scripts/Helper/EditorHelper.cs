using UnityEngine;

namespace NFramework
{
    public static class EditorHelper
    {
        public static string GetUniqueProjectPrefsKey(string key) => $"{Application.dataPath}_{key}";
    }
}
using UnityEditor;
using UnityEngine;

namespace NFramework.Editor
{
    public static class AboutNFrameworkMenuItem
    {
        [MenuItem("Tools/NFramework/About NFramework", priority = 1000)]
        public static void ShowAbout()
        {
            string message = $"NFramework\nVersion: {NFrameworkVersion.VERSION}\n\nA modular Unity framework for rapid game development.";
            Debug.Log($"[NFramework] Currently installed version: {NFrameworkVersion.VERSION}");
            EditorUtility.DisplayDialog("About NFramework", message, "OK");
        }
    }
}

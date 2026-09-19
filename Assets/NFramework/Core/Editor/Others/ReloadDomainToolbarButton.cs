using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

namespace NFramework.Editor
{
    public static class ReloadDomainToolbarButton
    {
        [MainToolbarElement("NFramework/Reload Domain", defaultDockPosition = MainToolbarDockPosition.Middle, defaultDockIndex = 1)]
        public static MainToolbarElement CreateToolbarButton()
        {
            Texture2D icon = EditorGUIUtility.IconContent("Refresh").image as Texture2D;
            var content = new MainToolbarContent(icon, "Reload Domain");
            return new MainToolbarButton(content, ReloadDomain);
        }

        public static void ReloadDomain()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogError("[Reload Domain] Cannot reload domain while in Play Mode.");
                return;
            }

            if (EditorApplication.isCompiling)
            {
                Debug.LogError("[Reload Domain] Cannot reload domain while scripts are compiling.");
                return;
            }

            EditorUtility.RequestScriptReload();
        }
    }
}

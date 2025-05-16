using UnityEditor;

namespace NFramework.Editor
{
    public static class PlayGameMenuItem
    {
        [MenuItem("NFramework/Play Game ^SPACE", priority = 0)]
        public static void PlayGame() => SceneSwitcherControl.PlayGame();
    }
}

using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace NFramework.Editor
{
    public class FastRestartEditor
    {
        [MenuItem("NFramework/Fast Restart Game")]
        private static void FastRestart()
        {
            EditorApplication.ExitPlaymode();

            CompilationPipeline.compilationFinished += CompilationPipeline_compilationFinished;

            CompilationPipeline.RequestScriptCompilation();

        }

        private static void CompilationPipeline_compilationFinished(object obj)
        {
            EditorApplication.EnterPlaymode();
        }
    }
}

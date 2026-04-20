using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TicTacToe.Editor
{
    public static class TicTacToeCleanupTools
    {
        [MenuItem("Tools/TicTacToe/Cleanup Missing Scripts (Open Scenes)")]
        public static void CleanupMissingScripts()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("TicTacToe: Stop Play mode before cleanup.");
                return;
            }

            var removedTotal = 0;
            for (var i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                var scene = EditorSceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;

                foreach (var root in scene.GetRootGameObjects())
                    removedTotal += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);
            }

            if (removedTotal > 0)
                Debug.Log($"TicTacToe: Removed {removedTotal} missing script component(s).");
            else
                Debug.Log("TicTacToe: No missing scripts found in open scenes.");
        }

        [MenuItem("Tools/TicTacToe/Cleanup Missing Scripts (Open Scenes)", true)]
        private static bool CleanupMissingScripts_Validate() => !EditorApplication.isPlayingOrWillChangePlaymode;
    }
}


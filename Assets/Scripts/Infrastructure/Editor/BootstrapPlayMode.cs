using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game.Infrastructure.Editor
{
    /// <summary>
    /// Every Play in the Editor starts from the Bootstrap scene, whatever scene is open (D118): content scenes need the
    /// root scope and run only through the bootstrapper. A tool may start one Play from another scene (the Level
    /// Editor's test, D131); leaving play mode makes Bootstrap the start scene again. Leaving play mode also returns
    /// to the scene that was open.
    /// </summary>
    [InitializeOnLoad]
    internal static class BootstrapPlayMode
    {
        private const string BootstrapScene = "Assets/Scenes/Bootstrap.unity";

        static BootstrapPlayMode()
        {
            // This also runs after the domain reload of entering play mode; the start scene is already chosen then.
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
                UseBootstrap();

            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredEditMode)
                    UseBootstrap();
            };
        }

        private static void UseBootstrap()
        {
            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(BootstrapScene);

            if (scene == null)
                Debug.LogWarning($"{BootstrapScene} is missing; Play starts from the open scene.");

            EditorSceneManager.playModeStartScene = scene;
        }
    }
}

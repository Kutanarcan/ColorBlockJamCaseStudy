using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game.Infrastructure.Editor
{
    /// <summary>
    /// Every Play in the Editor starts from the Bootstrap scene, whatever scene is open (D118): content scenes need the
    /// root scope and run only through the bootstrapper. Leaving play mode returns to the scene that was open.
    /// </summary>
    [InitializeOnLoad]
    internal static class BootstrapPlayMode
    {
        private const string BootstrapScene = "Assets/Scenes/Bootstrap.unity";

        static BootstrapPlayMode()
        {
            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(BootstrapScene);

            if (scene == null)
                Debug.LogWarning($"{BootstrapScene} is missing; Play starts from the open scene.");

            EditorSceneManager.playModeStartScene = scene;
        }
    }
}

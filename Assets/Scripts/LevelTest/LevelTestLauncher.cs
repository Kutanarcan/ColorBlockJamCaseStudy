using System;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Game.LevelTest
{
    /// <summary>
    /// Starts a level test (D131): leaves the level key for the LevelTest scene's scope and enters play mode from that
    /// scene, for this one Play only; leaving play mode puts the game's Bootstrap back as the start scene.
    /// </summary>
    public static class LevelTestLauncher
    {
        private const string LevelTestScene = "Assets/Scenes/LevelTest.unity";

        public static void Play(string levelKey)
        {
            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(LevelTestScene);

            if (scene == null)
                throw new InvalidOperationException($"{LevelTestScene} is missing.");

            new PlayRequest(new EditorPlayRequestStore()).Store(levelKey);
            EditorSceneManager.playModeStartScene = scene;
            EditorApplication.EnterPlaymode();
        }
    }
}

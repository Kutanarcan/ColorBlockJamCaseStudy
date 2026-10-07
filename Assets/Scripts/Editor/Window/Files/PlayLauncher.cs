using Game.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Game.LevelEditor
{
    /// <summary>
    /// Plays a saved level from the Level Editor (D75): leaves the key for the game, opens the Gameplay scene and
    /// enters play mode. From I5 it opens the Bootstrap scene instead.
    /// </summary>
    internal static class PlayLauncher
    {
        private const string GameplayScene = "Assets/Scenes/Gameplay.unity";

        public static void Play(string levelKey)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            new PlayRequest(new SessionStatePlayRequestStore()).Store(levelKey);
            EditorSceneManager.OpenScene(GameplayScene);
            EditorApplication.EnterPlaymode();
        }
    }
}

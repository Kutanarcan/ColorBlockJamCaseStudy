#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEngine.TestTools;

namespace Game.Tests.PlayMode
{
    /// <summary>
    /// Lets the Test Runner start play mode in its own scene. Every Editor Play starts from Bootstrap (D118), which
    /// would replace the runner's scene and stall the run; a test that loads Bootstrap does it itself. The Editor
    /// setting comes back with the next domain reload (<c>BootstrapPlayMode</c>).
    /// </summary>
    public sealed class TestRunnerStartScene : IPrebuildSetup
    {
        public void Setup() => EditorSceneManager.playModeStartScene = null;
    }
}
#endif

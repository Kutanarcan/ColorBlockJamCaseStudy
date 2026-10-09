using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Infrastructure
{
    /// <summary>
    /// Starts the level the start's choice points to (D131): selects it and opens Gameplay under the loading cover
    /// (D127). Home's Play and LevelComplete's Continue both start a level this way. One per scene scope, and one
    /// start per scope: a second press while the scene is changing does nothing. The change is not given the scene's
    /// token, since it unloads that scene and must not be cancelled by it.
    /// </summary>
    public sealed class LevelLauncher
    {
        private readonly SelectedLevel selected;
        private readonly ISceneLoader scenes;
        private bool launched;

        public LevelLauncher(SelectedLevel selected, ISceneLoader scenes)
        {
            this.selected = selected;
            this.scenes = scenes;
        }

        public void Launch()
        {
            if (launched)
                return;

            launched = true;
            selected.Select();
            scenes.ReplaceContentSceneAsync(SceneKeys.Gameplay, CancellationToken.None).Forget();
        }
    }
}

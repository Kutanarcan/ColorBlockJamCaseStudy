using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Infrastructure
{
    /// <summary>
    /// Where the player goes from a content scene (D127), always under the loading cover: a level (Home's Play,
    /// LevelComplete's Continue) or Home (LoseLife's Leave, the Play popup's X in Gameplay). A level is the start's
    /// choice (D131), so a level test replays its level and its Home shows the test's values (D130). One per scene
    /// scope and one change per scope: a second press while the scene is changing does nothing. The change is not
    /// given the scene's token, since it unloads that scene and must not be cancelled by it.
    /// </summary>
    public sealed class Navigation
    {
        private readonly SelectedLevel selected;
        private readonly ISceneLoader scenes;
        private bool leaving;

        public Navigation(SelectedLevel selected, ISceneLoader scenes)
        {
            this.selected = selected;
            this.scenes = scenes;
        }

        public void PlayLevel()
        {
            if (!Leave())
                return;

            selected.Select();
            scenes.ReplaceContentSceneAsync(SceneKeys.Gameplay, CancellationToken.None).Forget();
        }

        public void GoHome()
        {
            if (Leave())
                scenes.ReplaceContentSceneAsync(SceneKeys.Main, CancellationToken.None).Forget();
        }

        private bool Leave()
        {
            if (leaving)
                return false;

            leaving = true;

            return true;
        }
    }
}

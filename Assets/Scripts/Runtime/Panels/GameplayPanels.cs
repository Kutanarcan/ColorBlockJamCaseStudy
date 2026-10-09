using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Infrastructure;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// The Gameplay scene's level-end panels (D134), shown through the modal layer, which pauses play (D122).
    /// LevelComplete shows the reward already added at the win (D123); its Continue selects the next level and reopens
    /// the scene under the loading cover (D127). In a level test the next level is the tested one again (D130).
    /// LevelFail arrives in U5.
    /// </summary>
    public sealed class GameplayPanels : ILevelPanels, IDisposable
    {
        private readonly ModalLayer modals;
        private readonly LevelCompleteView levelComplete;
        private readonly LoadedConfig config;
        private readonly SelectedLevel selected;
        private readonly ISceneLoader scenes;
        private readonly IStep win;
        private bool continuing;

        public GameplayPanels(ModalLayer modals, LevelCompleteView levelComplete, LoadedConfig config,
            SelectedLevel selected, ISceneLoader scenes)
        {
            this.modals = modals;
            this.levelComplete = levelComplete;
            this.config = config;
            this.selected = selected;
            this.scenes = scenes;
            win = new WinStep(this);
            levelComplete.ContinueClicked += ContinueToNextLevel;
        }

        public IStep Win() => win;

        public IStep Fail() => new PopupPlaceholderStep("Fail");

        public void Dispose() => levelComplete.ContinueClicked -= ContinueToNextLevel;

        private UniTask ShowWinAsync(CancellationToken cancellation)
        {
            levelComplete.ShowReward(config.Value.WinReward);

            return modals.Open((RectTransform)levelComplete.transform).Play(cancellation);
        }

        private void ContinueToNextLevel()
        {
            // One press is enough; a second would start a second scene change.
            if (continuing)
                return;

            continuing = true;
            selected.Select();

            // Not this scene's token: the change unloads this scene, and must not be cancelled by it.
            scenes.ReplaceContentSceneAsync(SceneKeys.Gameplay, CancellationToken.None).Forget();
        }

        private sealed class WinStep : IStep
        {
            private readonly GameplayPanels panels;

            public WinStep(GameplayPanels panels) => this.panels = panels;

            public UniTask Play(CancellationToken cancellation) => panels.ShowWinAsync(cancellation);
        }
    }
}

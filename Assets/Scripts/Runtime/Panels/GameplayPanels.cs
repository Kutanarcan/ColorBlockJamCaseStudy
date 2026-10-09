using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core;
using Game.Infrastructure;
using Game.Meta;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// The Gameplay scene's level-end panels (D134), shown through the modal layer, which pauses play (D122).
    /// LevelComplete shows the reward already added at the win (D123); its Continue selects the next level and reopens
    /// the scene under the loading cover (D127). In a level test the next level is the tested one again (D130).
    /// LevelFail shows its fail kind's content and sells a continue (D124): paid, the level gets its seconds and plays
    /// on. Lives with the Gameplay scope; closing the scene stops whatever it was opening or closing.
    /// </summary>
    public sealed class GameplayPanels : ILevelPanels, ILevelFailActions, IDisposable
    {
        private readonly ModalLayer modals;
        private readonly LevelCompleteView levelComplete;
        private readonly LevelFailView levelFail;
        private readonly LoadedConfig config;
        private readonly Wallet wallet;
        private readonly SelectedLevel selected;
        private readonly ISceneLoader scenes;
        private readonly GameplayPopups popups;
        private readonly CancellationTokenSource life = new CancellationTokenSource();
        private readonly IStep win;
        private readonly IStep fail;
        private LevelSession session;
        private LevelFailPresenter failPresenter;
        private bool continuing;

        public GameplayPanels(ModalLayer modals, LevelCompleteView levelComplete, LevelFailView levelFail,
            LoadedConfig config, Wallet wallet, SelectedLevel selected, ISceneLoader scenes, GameplayPopups popups)
        {
            this.modals = modals;
            this.levelComplete = levelComplete;
            this.levelFail = levelFail;
            this.config = config;
            this.wallet = wallet;
            this.selected = selected;
            this.scenes = scenes;
            this.popups = popups;
            win = new PanelStep(this, true);
            fail = new PanelStep(this, false);
            levelComplete.ContinueClicked += ContinueToNextLevel;
        }

        /// <summary>The level being played, once the scene has built it, and the price of its continues.</summary>
        public void PlayWith(LevelSession level, ContinuePrice continuePrice)
        {
            session = level;
            failPresenter?.Dispose();
            failPresenter = new LevelFailPresenter(levelFail, wallet, continuePrice,
                Mathf.RoundToInt(config.Value.ContinueSeconds), this);
        }

        public IStep Win() => win;

        public IStep Fail() => fail;

        void ILevelFailActions.Continue(int seconds)
        {
            // The level plays again (Failed → Playing) before the panel has gone; the close resumes the ticks.
            session.AddTime(seconds);
            modals.Close(resume: true).Play(life.Token).Forget();
        }

        // The Play popup (Retry) takes the panel's place in the modal slot (U5.4); play stays paused.
        void ILevelFailActions.Close() => popups.OpenPlay();

        void ILevelFailActions.SeeBoard(bool seen) => modals.SeeThrough(seen).Play(life.Token).Forget();

        public void Dispose()
        {
            life.Cancel();
            life.Dispose();
            levelComplete.ContinueClicked -= ContinueToNextLevel;
            failPresenter?.Dispose();
        }

        private UniTask ShowWinAsync(CancellationToken cancellation)
        {
            levelComplete.ShowReward(config.Value.WinReward);

            return modals.Open((RectTransform)levelComplete.transform).Play(cancellation);
        }

        private UniTask ShowFailAsync(CancellationToken cancellation)
        {
            failPresenter.Show(ContentFor(session));

            return modals.Open((RectTransform)levelFail.transform).Play(cancellation);
        }

        /// <summary>
        /// The fail kind, read in Runtime (D124). Running out of time is the only one: a deadlock kind needs the
        /// solver, out of V2. A new kind adds its check here and its content on the view.
        /// </summary>
        private FailContent ContentFor(LevelSession failed) => levelFail.OutOfTime;

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

        private sealed class PanelStep : IStep
        {
            private readonly GameplayPanels panels;
            private readonly bool won;

            public PanelStep(GameplayPanels panels, bool won)
            {
                this.panels = panels;
                this.won = won;
            }

            public UniTask Play(CancellationToken cancellation) =>
                won ? panels.ShowWinAsync(cancellation) : panels.ShowFailAsync(cancellation);
        }
    }
}

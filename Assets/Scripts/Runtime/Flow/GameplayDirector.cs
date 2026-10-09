using Game.Core;
using Game.Meta;

namespace Game.Runtime
{
    /// <summary>
    /// Turns what the session reports into steps and owns the level's flow (D69). Two sequencers: exits run side by
    /// side and never block the player; the flow (win, fail) locks input and waits for the exits still running.
    /// The logic has already finished when a call arrives; the director only decides what plays and when. What a
    /// level-end panel shows is the scene's (<see cref="ILevelPanels"/>); the director only says when (D89).
    /// </summary>
    public sealed class GameplayDirector : ISessionObserver
    {
        private readonly IExitSteps exitSteps;
        private readonly InputLock input;
        private readonly Sequencer exits;
        private readonly Sequencer flow;
        private readonly float winPopupDelay;
        private readonly LevelCompletion completion;
        private readonly ILevelPanels panels;

        public GameplayDirector(IExitSteps exitSteps, InputLock input, Sequencer exits, Sequencer flow,
            float winPopupDelay, LevelCompletion completion, ILevelPanels panels)
        {
            this.exitSteps = exitSteps;
            this.input = input;
            this.exits = exits;
            this.flow = flow;
            this.winPopupDelay = winPopupDelay;
            this.completion = completion;
            this.panels = panels;
        }

        /// <summary>Restart and home: nothing from the last attempt keeps playing or flying (D71).</summary>
        public void CancelAll()
        {
            // The flow first, so the exits going idle cannot let a waiting popup through.
            flow.CancelAll();
            exits.CancelAll();
            exitSteps.ClearEffects();
        }

        // The drag moves the dragged block's view itself; no mechanic moves entities from a listener yet.
        public void OnEntityMoved(Entity entity, Cell offset) { }

        public void OnBlockExited(Block block, Direction direction) => exits.Run(exitSteps.For(block, direction));

        // Ice's count and its melt (the removal) show through the looks refreshed as each exiting block lands
        // (RefreshLooksStep), at the crunch rather than here, before the exit has played.
        public void OnModifierAdded(Entity entity, IModifier modifier) { }

        public void OnModifierRemoved(Entity entity, IModifier modifier) { }

        public void OnStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Won:
                    // The win is kept the moment it happens, before anything plays (D123).
                    completion.Record();
                    input.Lock(InputLockReason.Flow);
                    flow.Run(new StepSequence(
                        new WaitForIdleStep(exits),
                        new DelayStep(winPopupDelay),
                        panels.Win()));
                    break;
                case GameState.Failed:
                    input.Lock(InputLockReason.Flow);
                    flow.Run(new StepSequence(new WaitForIdleStep(exits), panels.Fail()));
                    break;
                default:
                    input.Unlock(InputLockReason.Flow);
                    break;
            }
        }
    }
}

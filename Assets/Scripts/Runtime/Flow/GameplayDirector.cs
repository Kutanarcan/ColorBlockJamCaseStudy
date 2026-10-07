using Game.Core;

namespace Game.Runtime
{
    /// <summary>
    /// Turns what the session reports into steps and owns the level's flow (D69). Two sequencers: exits run side by
    /// side and never block the player; the flow (win, fail) locks input and waits for the exits still running.
    /// The logic has already finished when a call arrives; the director only decides what plays and when.
    /// </summary>
    public sealed class GameplayDirector : ISessionObserver
    {
        private readonly IExitSteps exitSteps;
        private readonly InputLock input;
        private readonly Sequencer exits;
        private readonly Sequencer flow;
        private readonly float winPopupDelay;

        public GameplayDirector(IExitSteps exitSteps, InputLock input, Sequencer exits, Sequencer flow,
            float winPopupDelay)
        {
            this.exitSteps = exitSteps;
            this.input = input;
            this.exits = exits;
            this.flow = flow;
            this.winPopupDelay = winPopupDelay;
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

        // Ice's melt and count updates are P2.4 follow-ups; nothing adds or removes a look here yet.
        public void OnModifierAdded(Entity entity, IModifier modifier) { }

        public void OnModifierRemoved(Entity entity, IModifier modifier) { }

        public void OnStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Won:
                    input.Lock(InputLockReason.Flow);
                    flow.Run(new StepSequence(
                        new WaitForIdleStep(exits),
                        new DelayStep(winPopupDelay),
                        new PopupPlaceholderStep("Win")));
                    break;
                case GameState.Failed:
                    input.Lock(InputLockReason.Flow);
                    flow.Run(new StepSequence(new WaitForIdleStep(exits), new PopupPlaceholderStep("Fail")));
                    break;
                default:
                    input.Unlock(InputLockReason.Flow);
                    break;
            }
        }
    }
}

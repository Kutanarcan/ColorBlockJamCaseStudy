using Game.Core;

namespace Game.Runtime
{
    /// <summary>
    /// The level's lifecycle: ticks the session and the drag every frame, pauses (D70) and restarts (D71).
    /// Pause stops the session's ticks and locks input; <c>Time.timeScale</c> is never touched, so UI keeps animating.
    /// Restart cancels every step, drops the drag, rebuilds the views from the restarted session: nothing of the last
    /// attempt stays.
    /// </summary>
    public sealed class GameplayLoop : ITickable, IPausable
    {
        private readonly LevelSession session;
        private readonly GameplayDirector director;
        private readonly DragController drag;
        private readonly IBlocksView blocks;
        private readonly InputLock inputLock;

        public GameplayLoop(LevelSession session, GameplayDirector director, DragController drag, IBlocksView blocks,
            InputLock inputLock)
        {
            this.session = session;
            this.director = director;
            this.drag = drag;
            this.blocks = blocks;
            this.inputLock = inputLock;
        }

        public bool IsPaused { get; private set; }

        public void Tick(float deltaTime)
        {
            if (!IsPaused)
                session.Tick(deltaTime);

            drag.Tick(deltaTime);
        }

        public void Pause()
        {
            IsPaused = true;
            inputLock.Lock(InputLockReason.Pause);
        }

        public void Resume()
        {
            IsPaused = false;
            inputLock.Unlock(InputLockReason.Pause);
        }

        public void Restart()
        {
            // Steps first: their tweens stop before the views they move are released.
            director.CancelAll();
            drag.Cancel();
            blocks.Clear();
            session.Restart();
            blocks.Build(session.Board);
            Resume();
        }
    }
}

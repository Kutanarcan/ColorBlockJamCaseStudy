namespace Game.Core
{
    /// <summary>
    /// Runs one level: Playing → Won when the last block exits, Playing → Failed when time runs out,
    /// Failed → Playing on <see cref="AddTime"/>. <see cref="Restart"/> rebuilds everything from the definition.
    /// </summary>
    public sealed class LevelSession : ITickable
    {
        private readonly LevelData level;
        private readonly LevelTimer timer = new LevelTimer();
        private BlockMover mover;
        private ExitResolver exitResolver;

        /// <summary>Replaced on <see cref="Restart"/>; entity references from the old board are stale.</summary>
        public Board Board { get; private set; }

        public GameState State { get; private set; }
        public float RemainingTime => timer.Remaining;

        public LevelSession(LevelData level)
        {
            this.level = level;
            Restart();
        }

        public MoveResult TryMove(Block block, Direction direction)
        {
            if (State != GameState.Playing)
                return MoveResult.Blocked;

            MoveResult result = mover.TryMove(block, direction);

            if (result != MoveResult.Exited)
                return result;

            exitResolver.Resolve(block);

            if (Board.RemainingBlockCount == 0)
                State = GameState.Won;

            return result;
        }

        public void Tick(float deltaTime)
        {
            if (State != GameState.Playing)
                return;

            timer.Tick(deltaTime);

            if (timer.IsExpired)
                State = GameState.Failed;
        }

        /// <summary>Continue: adds time and resumes a failed level. When and how often is Runtime's decision.</summary>
        public void AddTime(float seconds)
        {
            if (State == GameState.Won)
                return;

            timer.Add(seconds);

            if (State == GameState.Failed && !timer.IsExpired)
                State = GameState.Playing;
        }

        public void Restart()
        {
            Board = BoardBuilder.Build(level);
            mover = new BlockMover(Board);
            exitResolver = new ExitResolver(Board);
            timer.Reset(level.TimeLimit);
            State = GameState.Playing;
        }
    }
}

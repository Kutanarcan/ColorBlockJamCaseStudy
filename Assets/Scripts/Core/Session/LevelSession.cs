namespace Game.Core
{
    public sealed class LevelSession : ITickable
    {
        private readonly LevelData level;
        private readonly LevelTimer timer = new LevelTimer();
        private BlockMover mover;
        private LevelCommands commands;
        private EventDispatcher events;
        private bool moveInProgress;

        public Board Board { get; private set; }

        public GameState State { get; private set; }
        public float RemainingTime => timer.Remaining;

        /// <summary>Player moves that moved a block or ended in an exit.</summary>
        public int MoveCount { get; private set; }

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

            if (result == MoveResult.Moved)
                moveInProgress = true;

            if (result != MoveResult.Exited)
                return result;

            events.RaiseExited(block);
            commands.Flush();
            CheckWin();
            EndMove();

            return result;
        }

        /// <summary>
        /// Runtime calls this when a player move ends (drag release). Counts only if a block actually moved;
        /// a move that ended in an exit was already counted, so a second call does nothing.
        /// </summary>
        public void CommitMove()
        {
            if (State != GameState.Playing || !moveInProgress)
                return;

            EndMove();
        }

        public void Tick(float deltaTime)
        {
            if (State != GameState.Playing)
                return;

            timer.Tick(deltaTime);
            events.RaiseTicked(deltaTime);

            if (commands.Flush())
                CheckWin();

            if (State == GameState.Playing && timer.IsExpired)
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
            commands = new LevelCommands(Board, this);
            events = new EventDispatcher(Board, commands);
            timer.Reset(level.TimeLimit);
            State = GameState.Playing;
            MoveCount = 0;
            moveInProgress = false;
        }

        internal void Fail()
        {
            if (State == GameState.Playing)
                State = GameState.Failed;
        }

        /// <summary>The move is counted even when the level was just won; listeners hear it only while playing.</summary>
        private void EndMove()
        {
            moveInProgress = false;
            MoveCount++;

            if (State != GameState.Playing)
                return;

            events.RaiseMoveCommitted();

            if (commands.Flush())
                CheckWin();
        }

        private void CheckWin()
        {
            if (State == GameState.Playing && WinRule.IsWon(Board))
                State = GameState.Won;
        }
    }
}

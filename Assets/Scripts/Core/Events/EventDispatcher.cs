namespace Game.Core
{
    public sealed class EventDispatcher
    {
        private readonly Board board;
        private readonly ILevelCommands commands;

        public EventDispatcher(Board board, ILevelCommands commands)
        {
            this.board = board;
            this.commands = commands;
        }

        public void RaiseExited(Block exited)
        {
            DispatchExited(exited, exited);

            for (int id = 0; id < board.EntityCount; id++)
            {
                Entity entity = board.GetEntity(id);

                if (IsOnBoard(entity))
                    DispatchExited(entity, exited);
            }
        }

        public void RaiseMoveCommitted()
        {
            for (int id = 0; id < board.EntityCount; id++)
            {
                Entity entity = board.GetEntity(id);

                if (!IsOnBoard(entity))
                    continue;

                for (int i = 0; i < entity.ModifierCount; i++)
                {
                    if (entity.GetModifier(i) is IMoveListener listener)
                        listener.OnMoveCommitted(entity, commands);
                }
            }
        }

        public void RaiseTicked(float deltaTime)
        {
            for (int id = 0; id < board.EntityCount; id++)
            {
                Entity entity = board.GetEntity(id);

                if (!IsOnBoard(entity))
                    continue;

                for (int i = 0; i < entity.ModifierCount; i++)
                {
                    if (entity.GetModifier(i) is ITickListener listener)
                        listener.OnTicked(entity, deltaTime, commands);
                }
            }
        }

        private void DispatchExited(Entity entity, Block exited)
        {
            for (int i = 0; i < entity.ModifierCount; i++)
            {
                if (entity.GetModifier(i) is IExitListener listener)
                    listener.OnExited(entity, exited, commands);
            }
        }

        private static bool IsOnBoard(Entity entity) => !(entity is Block block && block.IsExited);
    }
}

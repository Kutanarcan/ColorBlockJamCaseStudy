namespace Game.Core
{
    /// <summary>
    /// Hands the three events to the listeners of every entity still on the board, in id order, one pass each.
    /// Also applies the central durability rule on every exit: reduce, and remove when depleted.
    /// Effects go through <see cref="ILevelCommands"/>; the caller flushes them after the pass.
    /// </summary>
    public sealed class EventDispatcher
    {
        private readonly Board board;
        private readonly ILevelCommands commands;

        public EventDispatcher(Board board, ILevelCommands commands)
        {
            this.board = board;
            this.commands = commands;
        }

        /// <summary>The exiting block hears its own exit first, then every entity still on the board.</summary>
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
                IModifier modifier = entity.GetModifier(i);

                if (modifier is IDurable durable)
                    WearDown(entity, durable, exited);

                if (modifier is IExitListener listener)
                    listener.OnExited(entity, exited, commands);
            }
        }

        private void WearDown(Entity entity, IDurable durable, Block exited)
        {
            durable.Durability.Reduce(durable.AmountFor(exited));

            if (durable.Durability.IsDepleted)
                commands.RemoveModifier(entity, durable);
        }

        private static bool IsOnBoard(Entity entity) => !(entity is Block block && block.IsExited);
    }
}

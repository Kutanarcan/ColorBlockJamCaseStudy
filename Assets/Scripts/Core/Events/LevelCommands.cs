using System.Collections.Generic;

namespace Game.Core
{
    /// <summary>Records commands during an event pass; <see cref="Flush"/> applies them in recording order.</summary>
    internal sealed class LevelCommands : ILevelCommands
    {
        private readonly List<PendingCommand> pending = new List<PendingCommand>(16);
        private readonly Board board;
        private readonly LevelSession session;

        public LevelCommands(Board board, LevelSession session)
        {
            this.board = board;
            this.session = session;
        }

        public void AddModifier(Entity entity, IModifier modifier) =>
            pending.Add(new PendingCommand(CommandKind.AddModifier, entity, modifier));

        public void RemoveModifier(Entity entity, IModifier modifier) =>
            pending.Add(new PendingCommand(CommandKind.RemoveModifier, entity, modifier));

        public void MoveEntity(Entity entity, Cell offset) =>
            pending.Add(new PendingCommand(CommandKind.MoveEntity, entity, offset: offset));

        public void Fail() => pending.Add(new PendingCommand(CommandKind.Fail));

        public void AddTime(float seconds) => pending.Add(new PendingCommand(CommandKind.AddTime, seconds: seconds));

        /// <returns>True when at least one command was applied, so the level may have changed.</returns>
        public bool Flush()
        {
            int count = pending.Count;

            for (int i = 0; i < count; i++)
                Apply(pending[i]);

            pending.Clear();

            return count > 0;
        }

        private void Apply(in PendingCommand command)
        {
            switch (command.Kind)
            {
                case CommandKind.AddModifier:
                    command.Entity.AddModifier(command.Modifier);
                    break;
                case CommandKind.RemoveModifier:
                    command.Entity.RemoveModifier(command.Modifier);
                    break;
                case CommandKind.MoveEntity:
                    ApplyMove(command.Entity, command.Offset);
                    break;
                case CommandKind.Fail:
                    session.Fail();
                    break;
                case CommandKind.AddTime:
                    session.AddTime(command.Seconds);
                    break;
            }
        }

        private void ApplyMove(Entity entity, Cell offset)
        {
            if (entity is Block block && block.IsExited)
                return;

            if (board.CanPlace(entity, offset))
                board.MoveEntity(entity, offset);
        }
    }
}

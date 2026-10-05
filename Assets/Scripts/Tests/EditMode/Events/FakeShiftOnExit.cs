using Game.Core;

namespace Game.Tests.EditMode
{
    /// <summary>Fake: moves its owner by an offset on every exit. Shape of a jumping mechanic.</summary>
    internal sealed class FakeShiftOnExit : IExitListener
    {
        private readonly Cell offset;

        public FakeShiftOnExit(Cell offset) => this.offset = offset;

        public void OnExited(Entity owner, Block exited, ILevelCommands commands) =>
            commands.MoveEntity(owner, offset);
    }
}

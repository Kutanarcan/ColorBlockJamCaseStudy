using System.Collections.Generic;
using Game.Core;

namespace Game.Tests.Runtime
{
    /// <summary>Records the offset of every move the session reports, in order; ignores the rest.</summary>
    internal sealed class FakeMoveObserver : ISessionObserver
    {
        public List<Cell> Moves { get; } = new List<Cell>();

        public void OnEntityMoved(Entity entity, Cell offset) => Moves.Add(offset);

        public void OnBlockExited(Block block, Direction direction) { }

        public void OnModifierAdded(Entity entity, IModifier modifier) { }

        public void OnModifierRemoved(Entity entity, IModifier modifier) { }

        public void OnStateChanged(GameState state) { }
    }
}

using System.Collections.Generic;
using Game.Core;

namespace Game.Tests.EditMode
{
    /// <summary>Fake: writes every call as one line, in order, so a test reads what the view would hear.</summary>
    internal sealed class FakeSessionObserver : ISessionObserver
    {
        public List<string> Calls { get; } = new List<string>();

        public void OnEntityMoved(Entity entity, Cell offset) => Calls.Add($"Moved {entity.Id} {offset}");

        public void OnBlockExited(Block block, Direction direction) => Calls.Add($"Exited {block.Id} {direction}");

        public void OnModifierAdded(Entity entity, IModifier modifier) =>
            Calls.Add($"Added {entity.Id} {modifier.GetType().Name}");

        public void OnModifierRemoved(Entity entity, IModifier modifier) =>
            Calls.Add($"Removed {entity.Id} {modifier.GetType().Name}");

        public void OnStateChanged(GameState state) => Calls.Add($"State {state}");
    }
}

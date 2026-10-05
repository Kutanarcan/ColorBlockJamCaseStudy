using Game.Core;

namespace Game.Tests.EditMode
{
    /// <summary>Fake: makes its block win-exempt when a move is committed. Proves win is checked after commands.</summary>
    internal sealed class FakeBecomeExemptOnMove : IMoveListener
    {
        public void OnMoveCommitted(Entity owner, ILevelCommands commands) =>
            commands.AddModifier(owner, new FakeWinExempt());
    }
}

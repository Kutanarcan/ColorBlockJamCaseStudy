using Game.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// Fake: flips its owner's Exit capability on every exit by adding or removing a blocker.
    /// Shape of Curtain / Barrier, built only from AddModifier and RemoveModifier.
    /// </summary>
    internal sealed class FakeExitToggle : IExitListener
    {
        private readonly FakeExitBlocker blocker = new FakeExitBlocker();
        private bool closed;

        public void OnExited(Entity owner, Block exited, ILevelCommands commands)
        {
            if (closed)
                commands.RemoveModifier(owner, blocker);
            else
                commands.AddModifier(owner, blocker);

            closed = !closed;
        }
    }
}

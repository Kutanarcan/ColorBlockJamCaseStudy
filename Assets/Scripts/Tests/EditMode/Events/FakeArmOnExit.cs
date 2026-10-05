using Game.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// Fake: on the first exit, adds a <see cref="FakeFailOnExit"/> to its owner. Shows that commands are
    /// deferred: the added listener does not hear the exit that added it, only the next one.
    /// </summary>
    internal sealed class FakeArmOnExit : IExitListener
    {
        private bool armed;

        public void OnExited(Entity owner, Block exited, ILevelCommands commands)
        {
            if (armed)
                return;

            armed = true;
            commands.AddModifier(owner, new FakeFailOnExit());
        }
    }
}

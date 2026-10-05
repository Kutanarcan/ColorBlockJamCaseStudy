using Game.Core;

namespace Game.Tests.EditMode
{
    /// <summary>Fake: fails the level on any exit. Shape of a bomb-like mechanic.</summary>
    internal sealed class FakeFailOnExit : IExitListener
    {
        public void OnExited(Entity owner, Block exited, ILevelCommands commands) => commands.Fail();
    }
}

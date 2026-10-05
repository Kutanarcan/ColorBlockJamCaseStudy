using Game.Core;

namespace Game.Tests.EditMode
{
    /// <summary>Fake: adds time when its own block exits. Shape of Time Capsule.</summary>
    internal sealed class FakeTimeBonus : IExitListener
    {
        private readonly float seconds;

        public FakeTimeBonus(float seconds) => this.seconds = seconds;

        public void OnExited(Entity owner, Block exited, ILevelCommands commands)
        {
            if (exited == owner)
                commands.AddTime(seconds);
        }
    }
}

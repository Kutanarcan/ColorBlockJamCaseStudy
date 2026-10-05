using Game.Core;

namespace Game.Tests.EditMode
{
    /// <summary>Fake: records the time it has been ticked. Shape of a timed mechanic such as Bomb.</summary>
    internal sealed class FakeTickRecorder : ITickListener
    {
        public float Total { get; private set; }

        public void OnTicked(Entity owner, float deltaTime, ILevelCommands commands) => Total += deltaTime;
    }
}

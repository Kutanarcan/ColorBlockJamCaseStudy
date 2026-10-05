using Game.Core;

namespace Game.Tests.EditMode
{
    /// <summary>Fake: suspends Accept, so its door rejects every block. Shape of Iced Door / Locked Door.</summary>
    internal sealed class FakeDoorLock : ISuspender
    {
        public Capability Suspends => Capability.Accept;
    }
}

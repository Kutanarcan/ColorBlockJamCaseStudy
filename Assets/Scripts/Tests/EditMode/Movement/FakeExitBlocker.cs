using Game.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// Test fake: suspends Exit only, counts nothing. A block carrying it moves freely but cannot exit,
    /// which documents that suspending one capability leaves the others intact.
    /// </summary>
    internal sealed class FakeExitBlocker : ISuspender
    {
        public Capability Suspends => Capability.Exit;
    }
}

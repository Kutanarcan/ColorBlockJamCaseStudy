using Game.Core;

namespace Game.Tests.EditMode
{
    internal sealed class FakeExitBlockerData : ModifierData
    {
        public override IModifier ToModifier() => new FakeExitBlocker();
    }
}

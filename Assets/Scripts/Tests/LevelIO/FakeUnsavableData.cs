using Game.Core;

namespace Game.Tests.LevelIO
{
    /// <summary>Test-only: a modifier DTO that is never added to a catalog, so it cannot be saved.</summary>
    public sealed class FakeUnsavableData : ModifierData
    {
        public override string TypeName => "fake-unsavable";

        public override IModifier ToModifier() => new FakeTimed();

        public override void Write(IModifierWriter writer)
        {
        }

        public override void Read(IModifierReader reader)
        {
        }
    }
}

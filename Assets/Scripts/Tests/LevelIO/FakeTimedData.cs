using Game.Core;

namespace Game.Tests.LevelIO
{
    /// <summary>
    /// Test-only modifier DTO outside Game.Core. Proves a new modifier is saved and loaded with no LevelIO edit:
    /// it writes and reads its own fields, and the test adds it to the catalog.
    /// </summary>
    public sealed class FakeTimedData : ModifierData
    {
        public int Seconds;
        public Direction Facing;

        public override string TypeName => "fake-timed";

        public override IModifier ToModifier() => new FakeTimed();

        public override void Write(IModifierWriter writer)
        {
            writer.Int("seconds", Seconds);
            writer.Direction("facing", Facing);
        }

        public override void Read(IModifierReader reader)
        {
            Seconds = reader.Int("seconds");
            Facing = reader.Direction("facing");
        }
    }
}

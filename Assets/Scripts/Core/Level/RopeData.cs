namespace Game.Core
{
    public sealed class RopeData : ModifierData
    {
        public int ColorId;

        public override IModifier ToModifier() => new Rope(ColorId);
    }
}

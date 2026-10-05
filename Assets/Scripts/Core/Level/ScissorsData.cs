namespace Game.Core
{
    public sealed class ScissorsData : ModifierData
    {
        public int ColorId;

        public override IModifier ToModifier() => new Scissors(ColorId);
    }
}

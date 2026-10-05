namespace Game.Core
{
    public sealed class IceData : ModifierData
    {
        public int Count;

        public override IModifier ToModifier() => new Ice(Count);
    }
}

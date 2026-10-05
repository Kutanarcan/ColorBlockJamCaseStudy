namespace Game.Core
{
    public sealed class ArrowData : ModifierData
    {
        public Direction Direction;

        public override IModifier ToModifier() => new Arrow(Direction);
    }
}

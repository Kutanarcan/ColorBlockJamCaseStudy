namespace Game.Core
{
    public sealed class ArrowData : ModifierData
    {
        public Direction Direction;

        public override string TypeName => "arrow";

        public override IModifier ToModifier() => new Arrow(Direction);

        public override void Write(IModifierWriter writer) => writer.Direction("direction", Direction);

        public override void Read(IModifierReader reader) => Direction = reader.Direction("direction");
    }
}

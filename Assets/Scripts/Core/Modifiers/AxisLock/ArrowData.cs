namespace Game.Core
{
    public sealed class ArrowData : ModifierData
    {
        public Axis Axis;

        public override string TypeName => "arrow";

        public override IModifier ToModifier() => new Arrow(Axis);

        public override void Write(IModifierWriter writer) => writer.Axis("axis", Axis);

        public override void Read(IModifierReader reader) => Axis = reader.Axis("axis");
    }
}

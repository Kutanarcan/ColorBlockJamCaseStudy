namespace Game.Core
{
    public sealed class IceData : ModifierData
    {
        public int Count;

        public override string TypeName => "ice";

        public override IModifier ToModifier() => new Ice(Count);

        public override void Write(IModifierWriter writer) => writer.Int("count", Count);

        public override void Read(IModifierReader reader) => Count = reader.Int("count");

        public override Result Validate() =>
            Count > 0 ? Result.Success() : Result.Failure($"(ice) count must be greater than 0, was {Count}");
    }
}

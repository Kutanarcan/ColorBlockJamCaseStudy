namespace Game.Core
{
    public sealed class IceData : ModifierData
    {
        public int Count;

        public override IModifier ToModifier() => new Ice(Count);

        public override Result Validate() =>
            Count > 0 ? Result.Success() : Result.Failure($"(ice) count must be greater than 0, was {Count}");
    }
}

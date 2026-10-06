using Game.Core;

namespace Game.Tests.Additivity
{
    /// <summary>
    /// The saved form of <see cref="TurnBasedArrow"/>. It names itself, saves and loads its own fields and checks its
    /// own values, so LevelIO, load-time validation and the Level Editor handle it without a change.
    /// </summary>
    public sealed class TurnBasedArrowData : ModifierData
    {
        public Direction Direction;
        public int Count;

        public override string TypeName => "turn-based-arrow";

        public override IModifier ToModifier() => new TurnBasedArrow(Direction, Count);

        public override void Write(IModifierWriter writer)
        {
            writer.Direction("direction", Direction);
            writer.Int("count", Count);
        }

        public override void Read(IModifierReader reader)
        {
            Direction = reader.Direction("direction");
            Count = reader.Int("count");
        }

        public override Result Validate() =>
            Count > 0 ? Result.Success() : Result.Failure($"(turn-based-arrow) count must be greater than 0, was {Count}");
    }
}

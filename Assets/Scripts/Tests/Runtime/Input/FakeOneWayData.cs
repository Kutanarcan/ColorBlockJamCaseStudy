using Game.Core;

namespace Game.Tests.Runtime
{
    /// <summary>
    /// A block that moves in one direction only, for the previous-cell rule in <c>DragResolver.ClampToReachable</c>
    /// (no such mechanic is in the game since Arrow became an axis lock, D105). Never saved, so it writes and reads
    /// nothing.
    /// </summary>
    internal sealed class FakeOneWayData : ModifierData
    {
        public Direction Direction;

        public override string TypeName => "fake-one-way";

        public override IModifier ToModifier() => new OneWay(Direction);

        public override void Write(IModifierWriter writer) { }

        public override void Read(IModifierReader reader) { }

        private sealed class OneWay : IMoveConstraint
        {
            private readonly Direction direction;

            public OneWay(Direction direction) => this.direction = direction;

            public bool Allows(Direction other) => other == direction;
        }
    }
}

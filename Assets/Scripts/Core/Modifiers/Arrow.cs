namespace Game.Core
{
    /// <summary>The block moves, and therefore exits, only in this one direction.</summary>
    public sealed class Arrow : IMoveConstraint
    {
        public Direction Direction { get; }

        public Arrow(Direction direction) => Direction = direction;

        public bool Allows(Direction direction) => direction == Direction;
    }
}

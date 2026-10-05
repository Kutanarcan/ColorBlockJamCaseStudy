namespace Game.Core
{
    public sealed class Arrow : IMoveConstraint
    {
        public Direction Direction { get; }

        public Arrow(Direction direction) => Direction = direction;

        public bool Allows(Direction direction) => direction == Direction;
    }
}

namespace Game.Core
{
    /// <summary>Axis lock (D105): the block moves, and therefore exits, only along its axis, both ways.</summary>
    public sealed class Arrow : IMoveConstraint
    {
        public Axis Axis { get; }

        public Arrow(Axis axis) => Axis = axis;

        public bool Allows(Direction direction) => direction.ToAxis() == Axis;
    }
}

using Game.Core;

namespace Game.Tests.Additivity
{
    /// <summary>
    /// Turn Based Arrow, built outside Game.Core from existing parts only (D60): an <see cref="IMoveConstraint"/>
    /// locks the block to one direction, and an <see cref="IExitListener"/> wears a <see cref="Durability"/> down on
    /// every exit. When it is used up the modifier is removed, and what is left is a plain block that moves freely.
    /// </summary>
    public sealed class TurnBasedArrow : IMoveConstraint, IExitListener
    {
        public Direction Direction { get; }
        public Durability Durability { get; }

        public TurnBasedArrow(Direction direction, int count)
        {
            Direction = direction;
            Durability = new Durability(count);
        }

        public bool Allows(Direction direction) => direction == Direction;

        public void OnExited(Entity owner, Block exited, ILevelCommands commands) =>
            Durability.WearDown(owner, this, 1, commands);
    }
}

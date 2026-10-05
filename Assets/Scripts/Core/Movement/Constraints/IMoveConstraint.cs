namespace Game.Core
{
    public interface IMoveConstraint : IModifier
    {
        bool Allows(Direction direction);
    }
}

namespace Game.Core
{
    /// <summary>Narrows the directions a movable block may move in. Does not suspend movement.</summary>
    public interface IMoveConstraint : IModifier
    {
        bool Allows(Direction direction);
    }
}

namespace Game.Core
{
    /// <summary>
    /// Anything an entity can carry. A modifier only declares what it is through part interfaces
    /// (<see cref="ISuspender"/>, <see cref="IDurable"/>, <see cref="IMoveConstraint"/>);
    /// <see cref="Capabilities"/> and <see cref="EventDispatcher"/> apply the rules. Listener interfaces
    /// (<see cref="IExitListener"/>, <see cref="IMoveListener"/>, <see cref="ITickListener"/>) let a modifier react.
    /// A passive modifier implements none.
    /// </summary>
    public interface IModifier
    {
    }
}

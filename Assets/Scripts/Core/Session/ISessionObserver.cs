namespace Game.Core
{
    /// <summary>
    /// The one seam from logic to presentation (D68, D85). The session calls it after the logic step is done,
    /// so the view always hears a finished fact: a move, the exit, then its consequences, then the state.
    /// </summary>
    public interface ISessionObserver
    {
        void OnEntityMoved(Entity entity, Cell offset);
        void OnBlockExited(Block block, Direction direction);
        void OnModifierAdded(Entity entity, IModifier modifier);
        void OnModifierRemoved(Entity entity, IModifier modifier);
        void OnStateChanged(GameState state);
    }
}

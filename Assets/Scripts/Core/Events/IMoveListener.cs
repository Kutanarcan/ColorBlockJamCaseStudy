namespace Game.Core
{
    /// <summary>Reacts when a player move ends. Position-based mechanics read the board here.</summary>
    public interface IMoveListener : IModifier
    {
        void OnMoveCommitted(Entity owner, ILevelCommands commands);
    }
}

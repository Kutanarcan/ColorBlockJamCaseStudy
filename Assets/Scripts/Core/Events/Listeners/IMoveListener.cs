namespace Game.Core
{
    public interface IMoveListener : IModifier
    {
        void OnMoveCommitted(Entity owner, ILevelCommands commands);
    }
}

namespace Game.Core
{
    /// <summary>Reacts when a block exits. Called on the exiting block itself first, then on every other entity.</summary>
    public interface IExitListener : IModifier
    {
        void OnExited(Entity owner, Block exited, ILevelCommands commands);
    }
}

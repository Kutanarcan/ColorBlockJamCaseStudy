namespace Game.Core
{
    public interface IExitListener : IModifier
    {
        void OnExited(Entity owner, Block exited, ILevelCommands commands);
    }
}

namespace Game.Core
{
    public interface ITickListener : IModifier
    {
        void OnTicked(Entity owner, float deltaTime, ILevelCommands commands);
    }
}

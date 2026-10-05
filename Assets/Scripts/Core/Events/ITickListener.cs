namespace Game.Core
{
    /// <summary>Reacts to time passing while the level is being played.</summary>
    public interface ITickListener : IModifier
    {
        void OnTicked(Entity owner, float deltaTime, ILevelCommands commands);
    }
}

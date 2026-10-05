namespace Game.Core
{
    /// <summary>Driven by Runtime once per frame. Core never reads time on its own.</summary>
    public interface ITickable
    {
        void Tick(float deltaTime);
    }
}

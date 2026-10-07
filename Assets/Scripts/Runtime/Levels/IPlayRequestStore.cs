namespace Game.Runtime
{
    /// <summary>Where a <see cref="PlayRequest"/> waits while the Editor enters play mode. Empty means none.</summary>
    public interface IPlayRequestStore
    {
        string Read();

        void Write(string levelKey);
    }
}

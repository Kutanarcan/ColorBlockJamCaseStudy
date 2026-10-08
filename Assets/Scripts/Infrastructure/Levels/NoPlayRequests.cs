namespace Game.Infrastructure
{
    /// <summary>
    /// A player build's request store: only the Level Editor asks for a level (D118), so there is never one.
    /// </summary>
    public sealed class NoPlayRequests : IPlayRequestStore
    {
        public string Read() => "";

        public void Write(string levelKey)
        {
        }
    }
}

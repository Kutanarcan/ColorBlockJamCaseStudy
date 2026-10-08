namespace Game.Meta
{
    /// <summary>
    /// The coins a fresh wallet starts with. A start service (D131): the game reads its config, a level test its own
    /// test value (D130).
    /// </summary>
    public interface IStartingCoins
    {
        int Amount { get; }
    }
}

using Game.Meta;

namespace Game.Tests.Meta
{
    /// <summary>A fixed starting balance, as the config or the level test's config gives it.</summary>
    internal sealed class FakeStartingCoins : IStartingCoins
    {
        public FakeStartingCoins(int amount) => Amount = amount;

        public int Amount { get; }
    }
}

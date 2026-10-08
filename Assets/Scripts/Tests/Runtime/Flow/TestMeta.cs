using System.Collections.Generic;
using Game.Meta;

namespace Game.Tests.Runtime
{
    /// <summary>A wallet and progression over an in-memory save, for the director to record a win into.</summary>
    internal sealed class TestMeta
    {
        public TestMeta(int startCoins = 0, int reward = 0)
        {
            var store = new Store();
            Wallet = new Wallet(store, new Coins(startCoins));
            Progression = new Progression(store);
            Completion = new LevelCompletion(Wallet, Progression, reward);
        }

        public Wallet Wallet { get; }

        public Progression Progression { get; }

        public LevelCompletion Completion { get; }

        private sealed class Store : ISaveStore
        {
            private readonly Dictionary<string, object> sections = new Dictionary<string, object>();

            public T Load<T>(string key) where T : class, new() =>
                sections.TryGetValue(key, out object data) ? (T)data : new T();

            public void Save<T>(string key, T data) where T : class => sections[key] = data;
        }

        private sealed class Coins : IStartingCoins
        {
            public Coins(int amount) => Amount = amount;

            public int Amount { get; }
        }
    }
}

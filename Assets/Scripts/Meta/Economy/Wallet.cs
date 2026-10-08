using System;

namespace Game.Meta
{
    /// <summary>
    /// The player's coins (D79). A fresh wallet is given the starting coins once; every change is saved at once, so
    /// the balance survives leaving the game at any moment (brief 4.3). Spending only happens when the balance covers
    /// it: a wallet never goes below zero.
    /// </summary>
    public sealed class Wallet
    {
        public const string SaveKey = "wallet";

        private readonly ISaveStore store;
        private readonly IStartingCoins startingCoins;
        private WalletData data;

        public Wallet(ISaveStore store, IStartingCoins startingCoins)
        {
            this.store = store;
            this.startingCoins = startingCoins;
        }

        public int Coins => Data.coins;

        private WalletData Data => data ??= Open();

        public bool CanAfford(int amount) => Coins >= Positive(amount);

        public void Add(int amount)
        {
            Data.coins += Positive(amount);
            store.Save(SaveKey, data);
        }

        public bool TrySpend(int amount)
        {
            if (!CanAfford(amount))
                return false;

            data.coins -= amount;
            store.Save(SaveKey, data);

            return true;
        }

        private WalletData Open()
        {
            var opened = store.Load<WalletData>(SaveKey);

            if (opened.granted)
                return opened;

            opened.coins = startingCoins.Amount;
            opened.granted = true;
            store.Save(SaveKey, opened);

            return opened;
        }

        private static int Positive(int amount) =>
            amount > 0 ? amount : throw new ArgumentOutOfRangeException(nameof(amount), "Coin amounts are positive.");
    }
}

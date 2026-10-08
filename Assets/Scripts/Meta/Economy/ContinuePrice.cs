using System;

namespace Game.Meta
{
    /// <summary>
    /// The price of a continue on the fail popup (D124): the base price at every level start, restart included, and
    /// one step dearer after each continue (900 → 1900 → …). Paying takes the coins from the wallet or does nothing.
    /// Held by the level, never saved.
    /// </summary>
    public sealed class ContinuePrice
    {
        private readonly int basePrice;
        private readonly int step;

        public ContinuePrice(int basePrice, int step)
        {
            if (basePrice <= 0)
                throw new ArgumentOutOfRangeException(nameof(basePrice), "A continue costs something.");

            if (step < 0)
                throw new ArgumentOutOfRangeException(nameof(step), "A continue never gets cheaper.");

            this.basePrice = basePrice;
            this.step = step;
            Current = basePrice;
        }

        public int Current { get; private set; }

        /// <summary>A new level start: the next continue costs the base price again.</summary>
        public void Reset() => Current = basePrice;

        public bool TryPay(Wallet wallet)
        {
            if (!wallet.TrySpend(Current))
                return false;

            Current += step;

            return true;
        }
    }
}

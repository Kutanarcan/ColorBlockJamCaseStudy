using System;

namespace Game.Meta
{
    /// <summary>
    /// <see cref="Wallet"/>'s save section. <see cref="granted"/> marks that the starting coins were given, so a
    /// player who spends everything is not handed them again. Public fields, because the store writes fields.
    /// </summary>
    [Serializable]
    public sealed class WalletData
    {
        public int version = 1;
        public bool granted;
        public int coins;
    }
}

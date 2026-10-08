using System;

namespace Game.Meta
{
    /// <summary>
    /// What a won level gives (D123): the reward into the wallet and progression moved to the next level, both saved
    /// the moment the level is won, before any popup. Quitting on the win popup keeps the win.
    /// </summary>
    public sealed class LevelCompletion
    {
        private readonly Wallet wallet;
        private readonly Progression progression;

        public LevelCompletion(Wallet wallet, Progression progression, int reward)
        {
            if (reward < 0)
                throw new ArgumentOutOfRangeException(nameof(reward), "A win never costs coins.");

            this.wallet = wallet;
            this.progression = progression;
            Reward = reward;
        }

        /// <summary>The coins a win gives; the win popup shows it (U4).</summary>
        public int Reward { get; }

        public void Record()
        {
            if (Reward > 0)
                wallet.Add(Reward);

            progression.CompleteLevel();
        }
    }
}

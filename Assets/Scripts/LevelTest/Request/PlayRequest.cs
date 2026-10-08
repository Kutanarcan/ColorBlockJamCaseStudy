using System;

namespace Game.LevelTest
{
    /// <summary>
    /// The level the Level Editor asked to play (D75), handed from the Editor to the game across entering play mode.
    /// It is taken once, by the level test's root scope (D131), so a stale key never starts a later test.
    /// </summary>
    public sealed class PlayRequest
    {
        private readonly IPlayRequestStore store;

        public PlayRequest(IPlayRequestStore store) => this.store = store;

        public void Store(string levelKey)
        {
            if (string.IsNullOrEmpty(levelKey))
                throw new ArgumentException("A play request needs a level key.", nameof(levelKey));

            store.Write(levelKey);
        }

        public bool TryTake(out string levelKey)
        {
            levelKey = store.Read();

            if (string.IsNullOrEmpty(levelKey))
                return false;

            store.Write("");

            return true;
        }
    }
}

using System;

namespace Game.Meta
{
    /// <summary>
    /// How far the player got. It counts completed levels instead of storing an index, so the level to play is
    /// the count over the level list: after the last level it wraps to the first (D78), and a shorter list never
    /// leaves the save pointing past its end. The list itself is config, outside Meta. Its section is read on first
    /// use and written on every completed level.
    /// </summary>
    public sealed class Progression
    {
        public const string SaveKey = "progression";

        private readonly ISaveStore store;
        private ProgressionData data;

        public Progression(ISaveStore store) => this.store = store;

        /// <summary>The number the player sees; it keeps counting after the list wraps.</summary>
        public int LevelNumber => Data.levelsCompleted + 1;

        private ProgressionData Data => data ??= store.Load<ProgressionData>(SaveKey);

        public int LevelIndex(int levelCount)
        {
            if (levelCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(levelCount), "There must be at least one level.");

            return Data.levelsCompleted % levelCount;
        }

        public void CompleteLevel()
        {
            Data.levelsCompleted++;
            store.Save(SaveKey, data);
        }
    }
}

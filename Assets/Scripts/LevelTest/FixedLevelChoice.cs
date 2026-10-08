using System;
using Game.Infrastructure;

namespace Game.LevelTest
{
    /// <summary>The level test's choice: the tested level, at start, after a win and from Home (D130).</summary>
    public sealed class FixedLevelChoice : ILevelChoice
    {
        private readonly string key;

        public FixedLevelChoice(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("A level test needs a level key.", nameof(key));

            this.key = key;
        }

        public string Choose() => key;
    }
}

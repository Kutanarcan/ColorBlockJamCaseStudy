using System;

namespace Game.Meta
{
    /// <summary><see cref="Progression"/>'s save section. Public fields, because the store writes fields.</summary>
    [Serializable]
    public sealed class ProgressionData
    {
        public int version = 1;
        public int levelsCompleted;
    }
}

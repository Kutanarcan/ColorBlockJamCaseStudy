using System;

namespace Game.LevelIO
{
    /// <summary>Valid JSON that is not a level. Never leaves LevelIO: LevelJson.Parse turns it into a Result.</summary>
    internal sealed class LevelFormatException : Exception
    {
        public LevelFormatException(string message) : base(message)
        {
        }
    }
}

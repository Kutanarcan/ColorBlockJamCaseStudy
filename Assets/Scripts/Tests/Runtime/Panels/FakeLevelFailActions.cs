using System.Collections.Generic;
using Game.Runtime;

namespace Game.Tests.Runtime
{
    /// <summary>Does nothing; writes "continue N" and "close" to a log, in call order.</summary>
    internal sealed class FakeLevelFailActions : ILevelFailActions
    {
        public List<string> Log { get; } = new List<string>();

        public void Continue(int seconds) => Log.Add("continue " + seconds);

        public void Close() => Log.Add("close");
    }
}

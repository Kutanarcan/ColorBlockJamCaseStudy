using System.Collections.Generic;
using Game.Runtime;

namespace Game.Tests.Runtime
{
    /// <summary>Does nothing; writes "retry", "leave" and "close" to a log, in call order.</summary>
    internal sealed class FakeLoseLifeActions : ILoseLifeActions
    {
        public List<string> Log { get; } = new List<string>();

        public void Retry() => Log.Add("retry");

        public void Leave() => Log.Add("leave");

        public void Close() => Log.Add("close");
    }
}

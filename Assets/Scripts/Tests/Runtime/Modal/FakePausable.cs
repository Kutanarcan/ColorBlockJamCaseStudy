using System.Collections.Generic;
using Game.Runtime;

namespace Game.Tests.Runtime
{
    /// <summary>Pauses nothing; writes "pause" and "resume" to a log, in call order.</summary>
    internal sealed class FakePausable : IPausable
    {
        private readonly List<string> log;

        public FakePausable(List<string> log) => this.log = log;

        public void Pause() => log.Add("pause");

        public void Resume() => log.Add("resume");
    }
}

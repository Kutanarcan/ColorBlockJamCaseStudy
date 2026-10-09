using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Infrastructure;

namespace Game.Tests.Infrastructure
{
    /// <summary>Shows nothing; writes "show" and "hide" to a log, in call order.</summary>
    internal sealed class FakeLoadingCover : ILoadingCover
    {
        private readonly List<string> log;

        public FakeLoadingCover(List<string> log) => this.log = log;

        public UniTask ShowAsync(CancellationToken cancellation)
        {
            log.Add("show");

            return UniTask.CompletedTask;
        }

        public void Hide() => log.Add("hide");
    }
}

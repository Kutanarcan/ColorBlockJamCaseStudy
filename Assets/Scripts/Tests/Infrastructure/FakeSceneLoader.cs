using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Infrastructure;

namespace Game.Tests.Infrastructure
{
    /// <summary>Loads nothing; writes "replace key" and "unload" to a log, in call order.</summary>
    internal sealed class FakeSceneLoader : ISceneLoader
    {
        public List<string> Log { get; } = new List<string>();

        public UniTask ReplaceContentSceneAsync(string key, CancellationToken cancellation)
        {
            Log.Add("replace " + key);

            return UniTask.CompletedTask;
        }

        public UniTask UnloadContentSceneAsync(CancellationToken cancellation)
        {
            Log.Add("unload");

            return UniTask.CompletedTask;
        }
    }
}

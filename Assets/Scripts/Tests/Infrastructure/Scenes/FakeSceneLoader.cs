using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Infrastructure;

namespace Game.Tests.Infrastructure
{
    /// <summary>
    /// Loads nothing; writes "replace key" and "unload" to a log, in call order. The log can be shared with other
    /// fakes to check the order across them. A failing loader throws on replace, after logging it.
    /// </summary>
    internal sealed class FakeSceneLoader : ISceneLoader
    {
        public FakeSceneLoader(List<string> log = null) => Log = log ?? new List<string>();

        public List<string> Log { get; }

        public bool Fails { get; set; }

        public UniTask ReplaceContentSceneAsync(string key, CancellationToken cancellation)
        {
            Log.Add("replace " + key);

            if (Fails)
                throw new InvalidOperationException("The scene failed to load.");

            return UniTask.CompletedTask;
        }

        public UniTask UnloadContentSceneAsync(CancellationToken cancellation)
        {
            Log.Add("unload");

            return UniTask.CompletedTask;
        }
    }
}

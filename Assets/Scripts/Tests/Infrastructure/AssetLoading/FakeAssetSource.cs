using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Infrastructure;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Tests.Infrastructure
{
    /// <summary>
    /// Content the test finishes by hand: every load waits until <see cref="Finish"/>, so a test can close a scope
    /// while a load is still running. Continuations run inline, so no frame has to pass.
    /// </summary>
    internal sealed class FakeAssetSource : IAssetSource
    {
        private readonly Dictionary<string, UniTaskCompletionSource<IAssetHandle>> pending =
            new Dictionary<string, UniTaskCompletionSource<IAssetHandle>>();

        public List<FakeAssetHandle> Handles { get; } = new List<FakeAssetHandle>();

        public UniTask<IAssetHandle> LoadAsync<T>(string key, CancellationToken cancellation) where T : Object
        {
            var completion = new UniTaskCompletionSource<IAssetHandle>();
            pending.Add(key, completion);

            return completion.Task;
        }

        public FakeAssetHandle Finish(string key)
        {
            var handle = new FakeAssetHandle(new TextAsset(key));
            Handles.Add(handle);
            pending[key].TrySetResult(handle);
            pending.Remove(key);

            return handle;
        }
    }
}

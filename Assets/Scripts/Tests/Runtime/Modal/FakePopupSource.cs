using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Infrastructure;
using Object = UnityEngine.Object;

namespace Game.Tests.Runtime
{
    /// <summary>Hands out one prefab for every key at once; counts the loads and the releases.</summary>
    internal sealed class FakePopupSource : IAssetSource
    {
        private readonly Object prefab;

        public FakePopupSource(Object prefab) => this.prefab = prefab;

        public int Loads { get; private set; }

        public int Releases { get; private set; }

        public UniTask<IAssetHandle> LoadAsync<T>(string key, CancellationToken cancellation) where T : Object
        {
            Loads++;

            return UniTask.FromResult<IAssetHandle>(new Handle(this, prefab));
        }

        private sealed class Handle : IAssetHandle
        {
            private readonly FakePopupSource source;

            public Handle(FakePopupSource source, Object asset)
            {
                this.source = source;
                Asset = asset;
            }

            public Object Asset { get; }

            public void Release() => source.Releases++;
        }
    }
}

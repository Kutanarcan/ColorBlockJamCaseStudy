using Game.Infrastructure;
using Object = UnityEngine.Object;

namespace Game.Tests.Infrastructure
{
    /// <summary>A loaded asset that remembers how often it was released.</summary>
    internal sealed class FakeAssetHandle : IAssetHandle
    {
        public FakeAssetHandle(Object asset) => Asset = asset;

        public Object Asset { get; }

        public int Releases { get; private set; }

        public void Release() => Releases++;
    }
}

using Object = UnityEngine.Object;

namespace Game.Infrastructure
{
    /// <summary>One loaded asset and the right to release it.</summary>
    public interface IAssetHandle
    {
        Object Asset { get; }

        void Release();
    }
}

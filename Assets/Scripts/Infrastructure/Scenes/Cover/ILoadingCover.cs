using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Infrastructure
{
    /// <summary>
    /// The full-screen cover over every content scene change (D119). The scene loader shows it before the change;
    /// the scene that opens lifts it once it is built or has failed to build. A scene closed while building leaves
    /// it alone: whoever closed it owns the cover.
    /// </summary>
    public interface ILoadingCover
    {
        /// <summary>Covers the screen; finishes once nothing behind it can be seen.</summary>
        UniTask ShowAsync(CancellationToken cancellation);

        /// <summary>Uncovers the screen; does not wait for the fade.</summary>
        void Hide();
    }
}

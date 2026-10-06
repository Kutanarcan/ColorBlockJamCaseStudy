using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Runtime
{
    /// <summary>
    /// One unit of presentation (D69): a tween, a wait, a popup. Finishes when its effect is over; a cancelled
    /// token stops it where it is and ends it with <see cref="System.OperationCanceledException"/>.
    /// </summary>
    public interface IStep
    {
        UniTask Play(CancellationToken cancellation);
    }
}

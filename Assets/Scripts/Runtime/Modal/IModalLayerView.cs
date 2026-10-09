using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// What the modal layer does in Unity: the dim and the open / close look of a modal (a panel or popup root under
    /// the layer). Every animation ends at its final state, also when cancelled, so nothing is left half open.
    /// </summary>
    public interface IModalLayerView
    {
        /// <summary>Turns the modal on above everything else in the layer, at its closed look.</summary>
        void Show(RectTransform modal);

        /// <summary>Turns the modal off at once and puts its look back to rest.</summary>
        void Hide(RectTransform modal);

        /// <summary>Plays the modal's open or close look.</summary>
        UniTask Pop(RectTransform modal, bool open, CancellationToken cancellation);

        /// <summary>Fades the full-screen dim in or out; while in, it blocks the UI behind it.</summary>
        UniTask Dim(bool on, CancellationToken cancellation);
    }
}

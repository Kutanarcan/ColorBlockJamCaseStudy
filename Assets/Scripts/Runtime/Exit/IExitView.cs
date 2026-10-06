using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>What an exit drives: the block's cut, its motion, the row bursts and its end.</summary>
    public interface IExitView
    {
        void SetClipPlane(Vector4 plane);

        /// <summary>Moves the block to a board position; a cancel stops the motion where it is.</summary>
        UniTask MoveTo(Vector3 boardPosition, float duration, Ease ease, CancellationToken cancellation);

        void BurstRow(int row);

        void Hide();
    }
}

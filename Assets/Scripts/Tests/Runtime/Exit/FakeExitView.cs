using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Runtime;
using UnityEngine;

namespace Game.Tests.Runtime
{
    /// <summary>Records what the exit asked for, in order. Every move finishes at once.</summary>
    internal sealed class FakeExitView : IExitView
    {
        public List<string> Calls { get; } = new List<string>();
        public List<Vector3> Targets { get; } = new List<Vector3>();
        public List<Ease> Eases { get; } = new List<Ease>();
        public Vector4 ClipPlane { get; private set; }

        public void SetClipPlane(Vector4 plane)
        {
            ClipPlane = plane;
            Calls.Add("clip");
        }

        public UniTask MoveTo(Vector3 boardPosition, float duration, Ease ease, CancellationToken cancellation)
        {
            Targets.Add(boardPosition);
            Eases.Add(ease);
            Calls.Add("move");

            return UniTask.CompletedTask;
        }

        public void BurstRow(int row) => Calls.Add("burst " + row);

        public void Hide() => Calls.Add("hide");
    }
}

using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Runtime;
using UnityEngine;

namespace Game.Tests.Runtime
{
    /// <summary>Animates nothing; writes each call to a log ("show A", "pop A open", "dim on"), in order.</summary>
    internal sealed class FakeModalLayerView : IModalLayerView
    {
        public List<string> Log { get; } = new List<string>();

        public void Show(RectTransform modal) => Log.Add("show " + modal.name);

        public void Hide(RectTransform modal) => Log.Add("hide " + modal.name);

        public UniTask Pop(RectTransform modal, bool open, CancellationToken cancellation)
        {
            Log.Add("pop " + modal.name + (open ? " open" : " close"));

            return UniTask.CompletedTask;
        }

        public UniTask SeeThrough(RectTransform modal, bool through, CancellationToken cancellation)
        {
            Log.Add("see through " + modal.name + (through ? " on" : " off"));

            return UniTask.CompletedTask;
        }

        public UniTask Dim(bool on, CancellationToken cancellation)
        {
            Log.Add(on ? "dim on" : "dim off");

            return UniTask.CompletedTask;
        }
    }
}

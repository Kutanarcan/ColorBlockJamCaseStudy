using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>Placeholder popup: logs its name. U2 / U3 replace it with the popup service's open step.</summary>
    public sealed class PopupPlaceholderStep : IStep
    {
        private readonly string popup;

        public PopupPlaceholderStep(string popup) => this.popup = popup;

        public UniTask Play(CancellationToken cancellation)
        {
            Debug.Log($"[Popup placeholder] {popup}");

            return UniTask.CompletedTask;
        }
    }
}

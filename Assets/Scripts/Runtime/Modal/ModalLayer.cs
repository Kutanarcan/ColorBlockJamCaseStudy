using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// The one modal layer of a content scene (D135): a single slot shared by panels and popups, with the dim behind
    /// it. Opening while another modal is shown replaces it (the dim stays); closing empties the slot and lifts the
    /// dim. Opening and closing are steps, so the director runs them in its flow and a restart cancels them (D69);
    /// a cancelled step still ends at its final look. Panels and popups carry no dim and no animation of their own.
    /// The pause hook (D122): once a scene hands it its play, a shown modal pauses it, and closing resumes it only
    /// when asked to; a close that restarts or leaves the level does not resume. A scene without play (Home) hands
    /// none.
    /// </summary>
    public sealed class ModalLayer
    {
        private readonly IModalLayerView view;
        private RectTransform current;
        private IPausable play;

        public ModalLayer(IModalLayerView view) => this.view = view;

        public bool IsOpen => current != null;

        public IStep Open(RectTransform modal) => new OpenStep(this, modal);

        public IStep Close(bool resume) => new CloseStep(this, resume);

        /// <summary>The play to pause while a modal is shown; set once the scene has built it.</summary>
        public void PauseWhileShown(IPausable pausable) => play = pausable;

        private async UniTask OpenAsync(RectTransform modal, CancellationToken cancellation)
        {
            if (current == modal)
                return;

            RectTransform replaced = current;
            current = modal;
            play?.Pause();

            if (replaced != null)
                view.Hide(replaced);

            view.Show(modal);

            if (replaced == null)
                await UniTask.WhenAll(view.Dim(true, cancellation), view.Pop(modal, true, cancellation));
            else
                await view.Pop(modal, true, cancellation);
        }

        private async UniTask CloseAsync(bool resume, CancellationToken cancellation)
        {
            if (current == null)
                return;

            RectTransform closing = current;
            current = null;

            try
            {
                await UniTask.WhenAll(view.Dim(false, cancellation), view.Pop(closing, false, cancellation));
            }
            finally
            {
                view.Hide(closing);
            }

            if (resume)
                play?.Resume();
        }

        private sealed class OpenStep : IStep
        {
            private readonly ModalLayer layer;
            private readonly RectTransform modal;

            public OpenStep(ModalLayer layer, RectTransform modal)
            {
                this.layer = layer;
                this.modal = modal;
            }

            public UniTask Play(CancellationToken cancellation) => layer.OpenAsync(modal, cancellation);
        }

        private sealed class CloseStep : IStep
        {
            private readonly ModalLayer layer;
            private readonly bool resume;

            public CloseStep(ModalLayer layer, bool resume)
            {
                this.layer = layer;
                this.resume = resume;
            }

            public UniTask Play(CancellationToken cancellation) => layer.CloseAsync(resume, cancellation);
        }
    }
}

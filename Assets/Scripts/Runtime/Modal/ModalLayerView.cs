using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Runtime
{
    /// <summary>
    /// The modal layer's look, on <c>UIRoot</c>'s <c>ModalLayer</c> (D135). The dim is the layer's first child and
    /// fills the whole screen, notch included (no <c>SafeArea</c>); while on, its raycast blocks the UI behind it. A
    /// modal opens by scaling up from <see cref="closedScale"/> while it fades in, and closes by shrinking back while
    /// it fades out, so it is gone before it is turned off; all on unscaled time. The fade is the modal root's
    /// <see cref="CanvasGroup"/>, added when the root has none. A cancelled tween jumps to its end, so the layer never
    /// stays half open. Opening a modal is rare, so its tweens are not recycled.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ModalLayerView : MonoBehaviour, IModalLayerView
    {
        [Tooltip("Full-screen dim, the layer's first child; raycast target on.")]
        [SerializeField] private Image dim;

        [SerializeField, Range(0f, 1f)] private float dimAlpha = 0.7f;
        [SerializeField, Min(0f)] private float dimDuration = 0.2f;
        [SerializeField, Range(0.5f, 1f)] private float closedScale = 0.85f;
        [SerializeField, Min(0f)] private float openDuration = 0.25f;
        [SerializeField, Min(0f)] private float closeDuration = 0.2f;

        [Tooltip("Fade of the modal and the dim while the player looks through them (hold to see the board, D138).")]
        [SerializeField, Min(0f)] private float seeThroughDuration = 0.2f;

        private int dimChanges;
        private DOGetter<float> getDimAlpha;
        private DOSetter<float> setDimAlpha;

        public void Show(RectTransform modal)
        {
            modal.DOKill();
            modal.SetAsLastSibling();
            modal.localScale = new Vector3(closedScale, closedScale, 1f);
            GroupOf(modal).alpha = 0f;
            modal.gameObject.SetActive(true);
        }

        public void Hide(RectTransform modal)
        {
            modal.DOKill();
            modal.gameObject.SetActive(false);
            modal.localScale = Vector3.one;
            GroupOf(modal).alpha = 1f;
        }

        public UniTask Pop(RectTransform modal, bool open, CancellationToken cancellation)
        {
            modal.DOKill();

            CanvasGroup group = GroupOf(modal);
            float duration = open ? openDuration : closeDuration;

            return DOTween.Sequence()
                .Join(modal.DOScale(open ? 1f : closedScale, duration).SetEase(open ? Ease.OutBack : Ease.InQuad))
                .Join(DOTween.To(() => group.alpha, alpha => group.alpha = alpha, open ? 1f : 0f, duration)
                    .SetEase(open ? Ease.OutQuad : Ease.InQuad))
                .SetUpdate(true)
                .SetTarget(modal)
                .SetLink(modal.gameObject)
                .ToUniTask(TweenCancelBehaviour.CompleteAndCancelAwait, cancellation);
        }

        public async UniTask Dim(bool on, CancellationToken cancellation)
        {
            // A later call owns the dim; an earlier fade out that ends after it must not turn the dim off.
            int change = ++dimChanges;
            dim.DOKill();

            if (on)
                dim.gameObject.SetActive(true);

            try
            {
                await DOTween.To(getDimAlpha, setDimAlpha, on ? dimAlpha : 0f, dimDuration)
                    .SetUpdate(true)
                    .SetTarget(dim)
                    .SetLink(dim.gameObject)
                    .ToUniTask(TweenCancelBehaviour.CompleteAndCancelAwait, cancellation);
            }
            finally
            {
                if (!on && change == dimChanges)
                    dim.gameObject.SetActive(false);
            }
        }

        public UniTask SeeThrough(RectTransform modal, bool through, CancellationToken cancellation)
        {
            modal.DOKill();
            dim.DOKill();
            CanvasGroup group = GroupOf(modal);

            Tween panel = DOTween.To(() => group.alpha, alpha => group.alpha = alpha, through ? 0f : 1f,
                    seeThroughDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
                .SetTarget(modal)
                .SetLink(modal.gameObject);
            Tween shade = DOTween.To(getDimAlpha, setDimAlpha, through ? 0f : dimAlpha, seeThroughDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
                .SetTarget(dim)
                .SetLink(dim.gameObject);

            return UniTask.WhenAll(
                panel.ToUniTask(TweenCancelBehaviour.CompleteAndCancelAwait, cancellation),
                shade.ToUniTask(TweenCancelBehaviour.CompleteAndCancelAwait, cancellation));
        }

        private void Awake()
        {
            getDimAlpha = GetDimAlpha;
            setDimAlpha = SetDimAlpha;
            SetDimAlpha(0f);
            dim.gameObject.SetActive(false);
        }

        private void OnDestroy() => dim.DOKill();

        private static CanvasGroup GroupOf(RectTransform modal) =>
            modal.TryGetComponent(out CanvasGroup group) ? group : modal.gameObject.AddComponent<CanvasGroup>();

        private float GetDimAlpha() => dim.color.a;

        private void SetDimAlpha(float alpha)
        {
            Color color = dim.color;
            color.a = alpha;
            dim.color = color;
        }
    }
}

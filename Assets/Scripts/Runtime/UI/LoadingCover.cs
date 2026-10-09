using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using Game.Infrastructure;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// The loading cover's look (D119): its own canvas above <c>UIRoot</c> with a full-screen image and no
    /// <c>SafeArea</c>, faded through a <see cref="CanvasGroup"/> on unscaled time. It starts shown. While shown it
    /// blocks the UI behind it; hidden, its canvas is off, so the full-screen image costs nothing.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LoadingCover : MonoBehaviour, ILoadingCover
    {
        [Tooltip("This object's canvas; turned off while the cover is hidden.")]
        [SerializeField] private Canvas canvas;

        [Tooltip("This object's canvas group; its alpha is the fade.")]
        [SerializeField] private CanvasGroup group;

        [SerializeField, Min(0f)] private float showDuration = 0.2f;
        [SerializeField, Min(0f)] private float hideDuration = 0.3f;

        private DOGetter<float> getAlpha;
        private DOSetter<float> setAlpha;
        private TweenCallback turnOff;

        public async UniTask ShowAsync(CancellationToken cancellation)
        {
            group.blocksRaycasts = true;

            if (canvas.enabled && group.alpha >= 1f)
                return;

            canvas.enabled = true;
            await FadeTo(1f, showDuration).ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellation);
        }

        public void Hide()
        {
            group.blocksRaycasts = false;
            FadeTo(0f, hideDuration).OnComplete(turnOff);
        }

        private void Awake()
        {
            getAlpha = GetAlpha;
            setAlpha = SetAlpha;
            turnOff = TurnOff;
            canvas.enabled = true;
            group.alpha = 1f;
            group.blocksRaycasts = true;
        }

        private void OnDestroy() => group.DOKill();

        /// <summary>A show or a hide replaces the running fade, so the last call wins.</summary>
        private Tween FadeTo(float alpha, float duration)
        {
            group.DOKill();

            return DOTween.To(getAlpha, setAlpha, alpha, duration)
                .SetEase(Ease.Linear)
                .SetUpdate(true)
                .SetTarget(group);
        }

        private float GetAlpha() => group.alpha;

        private void SetAlpha(float alpha) => group.alpha = alpha;

        private void TurnOff() => canvas.enabled = false;
    }
}

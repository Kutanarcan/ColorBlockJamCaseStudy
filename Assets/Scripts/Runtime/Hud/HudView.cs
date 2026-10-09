using System;
using DG.Tweening;
using DG.Tweening.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Runtime
{
    /// <summary>
    /// The Gameplay HUD (§5 HUD, view contract): level number, countdown, coins, and the restart and pause buttons it
    /// reports. A dumb view: it formats nothing but its own labels and knows no logic. The booster bar is static.
    /// Texts are set through TextMeshPro's number overloads, so an update makes no garbage. The timer beats when told
    /// (D143): it grows and settles back, and with the alarm its red layer fades in and out with it. Each beat is one
    /// recyclable tween per part over getters and setters made once, so a beat a second makes no garbage (D109).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HudView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private PressButton restartButton;
        [SerializeField] private PressButton pauseButton;

        [Tooltip("This object's canvas group; its raycasts turn every HUD control on or off, boosters included.")]
        [SerializeField] private CanvasGroup controls;

        [Header("Timer beat (D143)")]
        [Tooltip("What grows on each beat: the timer, with what sits on it.")]
        [SerializeField] private RectTransform timerBeat;

        [Tooltip("The red layer of the timer, raycast off. Its color is set here; its alpha is 0 at rest and only the " +
                 "alarm raises it.")]
        [SerializeField] private Image timerAlarm;

        [SerializeField, Min(1f)] private float beatScale = 1.15f;

        [Tooltip("Seconds for one beat, out and back; shorter than the second between two beats.")]
        [SerializeField, Range(0.05f, 0.9f)] private float beatDuration = 0.3f;

        [SerializeField, Range(0f, 1f)] private float alarmAlpha = 0.8f;

        private Vector3 restScale;
        private DOGetter<Vector3> getScale;
        private DOSetter<Vector3> setScale;
        private DOGetter<float> getAlarmAlpha;
        private DOSetter<float> setAlarmAlpha;

        public event Action RestartClicked
        {
            add => restartButton.Clicked += value;
            remove => restartButton.Clicked -= value;
        }

        public event Action PauseClicked
        {
            add => pauseButton.Clicked += value;
            remove => pauseButton.Clicked -= value;
        }

        public void SetLevel(int number) => levelText.SetText("{0}", number);

        public void SetTime(int minutes, int seconds) => timerText.SetText("{0:00}:{1:00}", minutes, seconds);

        public void SetCoins(int coins) => coinText.SetText("{0}", coins);

        /// <summary>Off, no HUD control takes a touch; the HUD still shows everything.</summary>
        public void SetControlsOn(bool on) => controls.blocksRaycasts = on;

        /// <summary>One heartbeat of the timer: it grows and settles back; with the alarm, the red layer flashes too.</summary>
        public void BeatTimer(bool withAlarm)
        {
            float half = beatDuration * 0.5f;

            timerBeat.DOKill();
            timerBeat.localScale = restScale;
            Pulse(DOTween.To(getScale, setScale, restScale * beatScale, half), timerBeat);

            if (!withAlarm)
                return;

            timerAlarm.DOKill();
            SetAlarmAlpha(0f);
            Pulse(DOTween.To(getAlarmAlpha, setAlarmAlpha, alarmAlpha, half), timerAlarm);
        }

        private void Awake()
        {
            restScale = timerBeat.localScale;
            getScale = GetScale;
            setScale = SetScale;
            getAlarmAlpha = GetAlarmAlpha;
            setAlarmAlpha = SetAlarmAlpha;
            SetAlarmAlpha(0f);
        }

        /// <summary>Out and back once; no reference is kept, so the recycled tween is never touched again.</summary>
        private static void Pulse(Tween tween, Component target) =>
            tween.SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
                .SetTarget(target)
                .SetLink(target.gameObject)
                .SetRecyclable(true);

        private Vector3 GetScale() => timerBeat.localScale;

        private void SetScale(Vector3 scale) => timerBeat.localScale = scale;

        private float GetAlarmAlpha() => timerAlarm.color.a;

        private void SetAlarmAlpha(float alpha)
        {
            Color color = timerAlarm.color;
            color.a = alpha;
            timerAlarm.color = color;
        }
    }
}

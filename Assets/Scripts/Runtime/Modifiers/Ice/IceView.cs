using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// Dumb view of Ice (D102, D104): shows a count where it is told, shakes it when it drops and removes it when the
    /// ice melts; its material is what frozen parts wear.
    /// </summary>
    public sealed class IceView : MonoBehaviour, IIceView
    {
        [SerializeField] private TMP_Text count;
        [SerializeField] private Material ice;

        [Tooltip("How far the count grows at the peak of its shake.")]
        [SerializeField, Min(0f)] private float bumpStrength = 0.35f;

        [SerializeField, Min(0f)] private float bumpDuration = 0.3f;

        public Material Surface => ice;

        public void Place(Vector3 localPosition) => transform.localPosition = localPosition;

        /// <summary>Format overload: no string allocation.</summary>
        public void SetCount(int value) => count.SetText("{0}", value);

        public void Bump(int remaining)
        {
            SetCount(remaining);

            Transform target = count.transform;
            target.DOKill(complete: true);
            target.DOPunchScale(Vector3.one * bumpStrength, bumpDuration, 6, 0.5f)
                .SetTarget(target)
                .SetLink(target.gameObject);
        }

        public void Melt()
        {
            count.transform.DOKill();
            count.gameObject.SetActive(false);
        }

        private void OnDestroy() => count.transform.DOKill();
    }
}

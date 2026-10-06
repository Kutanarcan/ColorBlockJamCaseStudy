using TMPro;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>Dumb view of Ice (D102, D104): shows a count where it is told; its material is what frozen parts wear.</summary>
    public sealed class IceView : MonoBehaviour
    {
        [SerializeField] private TMP_Text count;
        [SerializeField] private Material ice;

        public Material Surface => ice;

        public void Place(Vector3 localPosition) => transform.localPosition = localPosition;

        /// <summary>Format overload: no string allocation.</summary>
        public void SetCount(int value) => count.SetText("{0}", value);
    }
}

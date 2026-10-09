using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Runtime
{
    /// <summary>
    /// An area that reports a press and its release, for "hold to …" (D138). Like <see cref="PressButton"/>, its
    /// touches come from a child raycast image; it has no look of its own. The release is reported wherever the pointer
    /// lifts, and also when the area turns off while held, so a hold never stays on.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HoldArea : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private bool held;

        public event Action Held;

        public event Action Released;

        public void OnPointerDown(PointerEventData eventData)
        {
            held = true;
            Held?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData) => Release();

        private void OnDisable() => Release();

        private void Release()
        {
            if (!held)
                return;

            held = false;
            Released?.Invoke();
        }
    }
}

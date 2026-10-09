using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Runtime
{
    /// <summary>
    /// Mouse button 0, and touch through Unity's mouse emulation (the project uses the old Input Manager). A press on
    /// the UI is not a press for the board (U1.4): the UI is ray cast once on the press frame, the same way the event
    /// system does, so it holds for touch too and does not depend on which runs first this frame. The event data and
    /// the hit list are reused, so a press makes no garbage.
    /// </summary>
    public sealed class MousePointerInput : IPointerInput
    {
        private readonly Camera camera;
        private readonly List<RaycastResult> uiHits = new List<RaycastResult>();
        private PointerEventData uiPointer;
        private EventSystem uiPointerOwner;

        public MousePointerInput(Camera camera) => this.camera = camera;

        public bool WasPressedThisFrame => Input.GetMouseButtonDown(0) && !IsOverUI();
        public bool IsPressed => Input.GetMouseButton(0);
        public Ray PointerRay => camera.ScreenPointToRay(Input.mousePosition);

        private bool IsOverUI()
        {
            EventSystem events = EventSystem.current;

            if (events == null)
                return false;

            if (uiPointerOwner != events)
            {
                uiPointer = new PointerEventData(events);
                uiPointerOwner = events;
            }

            uiPointer.position = Input.mousePosition;
            events.RaycastAll(uiPointer, uiHits);
            bool hit = uiHits.Count > 0;
            uiHits.Clear();

            return hit;
        }
    }
}

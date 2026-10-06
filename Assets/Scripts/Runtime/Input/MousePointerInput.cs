using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// Mouse button 0, and touch through Unity's mouse emulation (the project uses the old Input Manager).
    /// </summary>
    public sealed class MousePointerInput : IPointerInput
    {
        private readonly Camera camera;

        public MousePointerInput(Camera camera) => this.camera = camera;

        public bool WasPressedThisFrame => Input.GetMouseButtonDown(0);
        public bool IsPressed => Input.GetMouseButton(0);
        public Ray PointerRay => camera.ScreenPointToRay(Input.mousePosition);
    }
}

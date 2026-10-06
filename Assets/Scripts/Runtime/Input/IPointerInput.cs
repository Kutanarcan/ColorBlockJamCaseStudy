using UnityEngine;

namespace Game.Runtime
{
    /// <summary>The one pointer the drag reads: pressed this frame, held, and the ray it casts into the scene.</summary>
    public interface IPointerInput
    {
        bool WasPressedThisFrame { get; }
        bool IsPressed { get; }
        Ray PointerRay { get; }
    }
}

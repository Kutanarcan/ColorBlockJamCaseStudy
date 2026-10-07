using Game.Runtime;
using UnityEngine;

namespace Game.Tests.Runtime
{
    /// <summary>A pointer the test sets by hand; untouched it is never pressed.</summary>
    internal sealed class FakePointerInput : IPointerInput
    {
        public bool WasPressedThisFrame { get; set; }
        public bool IsPressed { get; set; }
        public Ray PointerRay { get; set; }
    }
}

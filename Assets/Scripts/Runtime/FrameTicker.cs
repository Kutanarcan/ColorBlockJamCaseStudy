using Game.Core;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>Forwards Unity's frame to the gameplay's tickable; nothing else runs in <c>Update</c>. P7.2 adds pause.</summary>
    public sealed class FrameTicker : MonoBehaviour
    {
        private ITickable tickable;

        public void Initialize(ITickable tickable) => this.tickable = tickable;

        private void Update() => tickable?.Tick(Time.deltaTime);
    }
}

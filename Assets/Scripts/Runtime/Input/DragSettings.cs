using System;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>How a dragged block looks: how high it rises, and how long it takes to snap onto its cell.</summary>
    [Serializable]
    public struct DragSettings
    {
        [SerializeField, Min(0f)] private float liftHeight;
        [SerializeField, Min(0f)] private float snapDuration;

        public DragSettings(float liftHeight, float snapDuration)
        {
            this.liftHeight = liftHeight;
            this.snapDuration = snapDuration;
        }

        public float LiftHeight => liftHeight;
        public float SnapDuration => snapDuration;
    }
}

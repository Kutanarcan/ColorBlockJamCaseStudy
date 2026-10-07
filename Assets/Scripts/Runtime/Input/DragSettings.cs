using System;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// How a dragged block feels: how high it rises, how long it takes to snap onto its cell, and how far toward its
    /// door the pointer has to pull before it exits (D111).
    /// </summary>
    [Serializable]
    public struct DragSettings
    {
        [SerializeField, Min(0f)] private float liftHeight;
        [SerializeField, Min(0f)] private float snapDuration;
        [Tooltip("Cells toward an open door that make the block exit. 0.5 = only where the pointer rounds onto the door.")]
        [SerializeField, Range(0.05f, 0.5f)] private float exitThreshold;

        public DragSettings(float liftHeight, float snapDuration, float exitThreshold)
        {
            this.liftHeight = liftHeight;
            this.snapDuration = snapDuration;
            this.exitThreshold = exitThreshold;
        }

        public float LiftHeight => liftHeight;
        public float SnapDuration => snapDuration;
        public float ExitThreshold => exitThreshold;
    }
}

using System;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>How an exit moves (FINDINGS tuning): the snap onto the cell, the slide speed, where the cut sits.</summary>
    [Serializable]
    public struct ExitSettings
    {
        [SerializeField, Min(0f)] private float snapDuration;
        [SerializeField, Min(0.01f)] private float slideSpeed;
        [Tooltip("Cut distance past the front edge; 0.5 = the middle of the door piece.")]
        [SerializeField, Min(0f)] private float clipOffset;

        public ExitSettings(float snapDuration, float slideSpeed, float clipOffset)
        {
            this.snapDuration = snapDuration;
            this.slideSpeed = slideSpeed;
            this.clipOffset = clipOffset;
        }

        public float SnapDuration => snapDuration;
        public float SlideSpeed => slideSpeed;
        public float ClipOffset => clipOffset;
    }
}

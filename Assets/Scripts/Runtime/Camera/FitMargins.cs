using System;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// Screen space the board must leave free, as shares of the screen (0..1), measured inward from the safe area:
    /// the HUD above and below, and air on both sides.
    /// </summary>
    [Serializable]
    public struct FitMargins
    {
        [SerializeField, Range(0f, 0.4f)] private float top;
        [SerializeField, Range(0f, 0.4f)] private float bottom;
        [SerializeField, Range(0f, 0.2f)] private float side;

        public FitMargins(float top, float bottom, float side)
        {
            this.top = top;
            this.bottom = bottom;
            this.side = side;
        }

        public float Top => top;
        public float Bottom => bottom;
        public float Side => side;
    }
}

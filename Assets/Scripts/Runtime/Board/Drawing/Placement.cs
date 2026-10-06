using UnityEngine;

namespace Game.Runtime
{
    /// <summary>Where one board piece goes: position in board space and Y rotation.</summary>
    public readonly struct Placement
    {
        public Vector3 Position { get; }
        public float Yaw { get; }

        public Placement(Vector3 position, float yaw)
        {
            Position = position;
            Yaw = yaw;
        }

        public override string ToString() => $"{Position} yaw {Yaw}";
    }
}

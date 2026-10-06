using System;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// How an exit row bursts: how many particles per cell, where they spawn and how they fly. Ranges are (min, max).
    /// Defaults are the values tuned on the prototype's particle prefab.
    /// </summary>
    [Serializable]
    public struct BurstTuning
    {
        [SerializeField, Min(1)] private int particlesPerCell;
        [SerializeField, Min(0f)] private float cellInset;
        [SerializeField] private Vector2 spawnHeight;
        [SerializeField] private Vector2 particleSize;
        [SerializeField] private Vector2 forwardSpeed;
        [SerializeField] private Vector2 upwardSpeed;
        [SerializeField, Min(0f)] private float sidewaysSpeed;
        [SerializeField] private Vector2 lifetime;

        public int ParticlesPerCell => particlesPerCell;
        public float CellInset => cellInset;
        public Vector2 SpawnHeight => spawnHeight;
        public Vector2 ParticleSize => particleSize;
        public Vector2 ForwardSpeed => forwardSpeed;
        public Vector2 UpwardSpeed => upwardSpeed;
        public float SidewaysSpeed => sidewaysSpeed;
        public Vector2 Lifetime => lifetime;
    }
}

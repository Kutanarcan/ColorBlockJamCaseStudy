using System.Collections.Generic;
using UnityEngine;

namespace Game.Prototype
{
    // Put this on a Particle System prefab and call Spawn when the block's visual exit finishes.
    // currentCells are the block's logical cells; logicalOffset is how far its root has moved since it was built.
    [RequireComponent(typeof(ParticleSystem))]
    public sealed class BlockExitParticles : MonoBehaviour
    {
        static readonly int ColorId = Shader.PropertyToID("_Color");

        [Header("Amount")]
        [SerializeField] int particlesPerCell = 12;
        [SerializeField] int maxParticles = 192;

        [Header("Spawn Volume")]
        [SerializeField] float cellInset = 0.15f;
        [SerializeField] Vector2 spawnHeight = new Vector2(0.1f, 0.8f);
        [SerializeField] Vector2 particleSize = new Vector2(0.16f, 0.32f);

        [Header("Motion")]
        [SerializeField] Vector2 forwardSpeed = new Vector2(2.5f, 4.5f);
        [SerializeField] Vector2 upwardSpeed = new Vector2(0.8f, 1.8f);
        [SerializeField] float sidewaysSpeed = 0.9f;
        [SerializeField] Vector2 lifetime = new Vector2(0.45f, 0.8f);

        // Pass the transform carrying the block visuals at the instant they finish entering the door.
        // The effect owns its particles, so the block can be hidden after this call.
        public void Spawn(Transform visualRoot, IReadOnlyList<Vector2Int> currentCells,
            Vector2Int logicalOffset, Vector2Int exitDirection, Color blockColor, float cellSize)
        {
            if (visualRoot == null || currentCells == null || currentCells.Count == 0 ||
                exitDirection == Vector2Int.zero || cellSize <= 0f) return;

            var effect = Instantiate(this, visualRoot.position, Quaternion.identity);
            effect.Emit(visualRoot, currentCells, logicalOffset, exitDirection, blockColor, cellSize);
        }

        void Emit(Transform visualRoot, IReadOnlyList<Vector2Int> currentCells,
            Vector2Int logicalOffset, Vector2Int exitDirection, Color blockColor, float cellSize)
        {
            var system = GetComponent<ParticleSystem>();
            int count = Mathf.Min(Mathf.Max(1, maxParticles),
                currentCells.Count * Mathf.Max(1, particlesPerCell));

            // The prefab loops a one-cell preview. Clear and disable that emission on the spawned copy.
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = system.main;
            main.loop = false;
            main.playOnAwake = false;
            main.maxParticles = count;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = system.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[0]);
            var shape = system.shape;
            shape.enabled = false;
            var velocity = system.velocityOverLifetime;
            velocity.enabled = false;

            // Standard ignores particle (vertex) color, so tint this copy's material instead.
            var props = new MaterialPropertyBlock();
            var particleRenderer = GetComponent<ParticleSystemRenderer>();
            particleRenderer.GetPropertyBlock(props);
            props.SetColor(ColorId, blockColor);
            particleRenderer.SetPropertyBlock(props);

            var forward = visualRoot.TransformDirection(
                new Vector3(exitDirection.x, 0f, exitDirection.y)).normalized;
            var up = visualRoot.up;
            var sideways = Vector3.Cross(up, forward).normalized;
            float halfWidth = Mathf.Max(0f, cellSize * 0.5f - cellInset);

            system.Play();
            for (int i = 0; i < count; i++)
            {
                // Spread a limited particle budget across the entire footprint, including long blocks.
                int cellIndex = Mathf.FloorToInt((i + 0.5f) * currentCells.Count / count);
                var cell = currentCells[cellIndex] - logicalOffset;
                var localPoint = new Vector3(
                    (cell.x + 0.5f) * cellSize + Random.Range(-halfWidth, halfWidth),
                    Range(spawnHeight),
                    (cell.y + 0.5f) * cellSize + Random.Range(-halfWidth, halfWidth));

                var particle = new ParticleSystem.EmitParams
                {
                    position = visualRoot.TransformPoint(localPoint),
                    velocity = forward * Range(forwardSpeed) + up * Range(upwardSpeed) +
                               sideways * Random.Range(-sidewaysSpeed, sidewaysSpeed),
                    startColor = blockColor,
                    startSize = Range(particleSize),
                    startLifetime = Range(lifetime),
                    rotation3D = new Vector3(Random.Range(0f, 360f), Random.Range(0f, 360f),
                        Random.Range(0f, 360f))
                };
                system.Emit(particle, 1);
            }

            Destroy(gameObject, Mathf.Max(lifetime.x, lifetime.y) + 0.25f);
        }

        static float Range(Vector2 limits) =>
            Random.Range(Mathf.Min(limits.x, limits.y), Mathf.Max(limits.x, limits.y));
    }
}

using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// Dumb view of one exit particle emitter (FINDINGS). One instance per palette color, made once and reused, so an
    /// exit never instantiates anything: the color comes from the shared block material, not a property block.
    /// </summary>
    public sealed class ExitParticles : MonoBehaviour
    {
        [SerializeField] private ParticleSystem system;
        [SerializeField] private ParticleSystemRenderer particleRenderer;
        [SerializeField] private BurstTuning tuning;

        public BurstTuning Tuning => tuning;

        /// <summary>World-space, no emission of its own: particles appear only through <see cref="Emit"/>.</summary>
        public void Prepare(Material material, int maxParticles)
        {
            particleRenderer.sharedMaterial = material;
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = system.main;
            main.loop = false;
            main.playOnAwake = false;
            main.maxParticles = maxParticles;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = system.emission;
            emission.enabled = false;
            ParticleSystem.ShapeModule shape = system.shape;
            shape.enabled = false;
            ParticleSystem.VelocityOverLifetimeModule velocity = system.velocityOverLifetime;
            velocity.enabled = false;

            system.Play();
        }

        public void Emit(Vector3 position, Vector3 velocity, Vector3 rotation, float size, float lifetime)
        {
            var particle = new ParticleSystem.EmitParams
            {
                position = position,
                velocity = velocity,
                rotation3D = rotation,
                startSize = size,
                startLifetime = lifetime
            };

            system.Emit(particle, 1);
        }
    }
}

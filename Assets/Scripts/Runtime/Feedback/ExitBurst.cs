using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace Game.Runtime
{
    /// <summary>
    /// Bursts particles from an exit row as it reaches the cut (FINDINGS). Makes one emitter per palette color once,
    /// with that color's shared block material; every exit reuses them, so a burst allocates nothing.
    /// </summary>
    public sealed class ExitBurst
    {
        private const int MaxParticlesPerColor = 1000;

        private readonly ExitParticles[] emitters;
        private readonly Random random;

        public ExitBurst(ExitParticles prefab, PaletteMaterials materials, int colorCount, Transform root, int seed)
        {
            emitters = new ExitParticles[colorCount];
            random = new Random(seed);

            for (int i = 0; i < colorCount; i++)
            {
                emitters[i] = Object.Instantiate(prefab, root);
                emitters[i].Prepare(materials.Block(i), MaxParticlesPerColor);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < emitters.Length; i++)
                emitters[i].Clear();
        }

        public void Emit(ExitPath path, int row, int colorId)
        {
            ExitParticles emitter = emitters[colorId];
            BurstTuning tuning = emitter.Tuning;
            Vector3 sideways = Vector3.Cross(Vector3.up, path.Normal);
            float halfWidth = Mathf.Max(0f, BoardLayout.CellSize * 0.5f - tuning.CellInset);

            for (int i = 0; i < path.CellCount; i++)
            {
                if (path.RowOf(path.GetCell(i)) != row)
                    continue;

                Vector3 center = path.CenterAtCut(path.GetCell(i));

                for (int j = 0; j < tuning.ParticlesPerCell; j++)
                {
                    Vector3 position = center + sideways * Range(-halfWidth, halfWidth)
                                              + Vector3.up * Range(tuning.SpawnHeight);
                    Vector3 velocity = path.Normal * Range(tuning.ForwardSpeed)
                                       + Vector3.up * Range(tuning.UpwardSpeed)
                                       + sideways * Range(-tuning.SidewaysSpeed, tuning.SidewaysSpeed);
                    var rotation = new Vector3(Range(0f, 360f), Range(0f, 360f), Range(0f, 360f));

                    emitter.Emit(position, velocity, rotation, Range(tuning.ParticleSize), Range(tuning.Lifetime));
                }
            }
        }

        private float Range(Vector2 limits) => Range(limits.x, limits.y);

        private float Range(float min, float max) => min + (float)random.NextDouble() * (max - min);
    }
}

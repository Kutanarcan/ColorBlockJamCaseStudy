using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Runtime
{
    /// <summary>
    /// One shared block material and one shared door material per palette color (D74, D95), made once from
    /// the kit's template materials. Every piece of a color uses the same instance, so pieces stay batchable.
    /// The owner disposes it when the level's scope closes.
    /// </summary>
    public sealed class PaletteMaterials : IDisposable
    {
        private readonly Material[] blocks;
        private readonly Material[] doors;

        public PaletteMaterials(Palette palette, Material blockTemplate, Material doorTemplate)
        {
            blocks = new Material[palette.Count];
            doors = new Material[palette.Count];

            for (int i = 0; i < palette.Count; i++)
            {
                blocks[i] = Tinted(blockTemplate, palette.ColorOf(i), $"Block_{i}");
                doors[i] = Tinted(doorTemplate, palette.ColorOf(i), $"Door_{i}");
            }
        }

        public Material Block(int colorId) => blocks[colorId];
        public Material Door(int colorId) => doors[colorId];

        public void Dispose()
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                Release(blocks[i]);
                Release(doors[i]);
            }
        }

        private static Material Tinted(Material template, Color color, string name) =>
            new Material(template) { name = name, color = color };

        /// <summary>EditMode tests run without play mode, where only DestroyImmediate is allowed.</summary>
        private static void Release(Material material)
        {
            if (Application.isPlaying)
                Object.Destroy(material);
            else
                Object.DestroyImmediate(material);
        }
    }
}

using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Runtime
{
    /// <summary>
    /// The level's shared materials, made once from the kit's templates (D95) and instanced (P8.2). Blocks share one
    /// material for every color; their color goes per instance through a property block (D108), so pieces of one mesh
    /// draw together whatever their color. Doors keep one material per color. The owner disposes it with the level.
    /// </summary>
    public sealed class PaletteMaterials : IDisposable
    {
        private readonly Palette palette;
        private readonly Material[] doors;

        public Material Block { get; }

        public PaletteMaterials(Palette palette, Material blockTemplate, Material doorTemplate)
        {
            this.palette = palette;
            Block = Instanced(blockTemplate, Color.white, "Block_Instanced");
            doors = new Material[palette.Count];

            for (int i = 0; i < palette.Count; i++)
                doors[i] = Instanced(doorTemplate, palette.ColorOf(i), $"Door_{i}");
        }

        public Color BlockColor(int colorId) => palette.ColorOf(colorId);

        public Material Door(int colorId) => doors[colorId];

        public void Dispose()
        {
            Release(Block);

            for (int i = 0; i < doors.Length; i++)
                Release(doors[i]);
        }

        private static Material Instanced(Material template, Color color, string name) =>
            new Material(template) { name = name, color = color, enableInstancing = true };

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

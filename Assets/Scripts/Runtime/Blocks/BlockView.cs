using System.Collections.Generic;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// One block on screen: its root (moved as one), its pooled parts and the views attached to it. The root sits at
    /// the board origin and parts at the block's cells, so a move is only a root offset
    /// (AlgorithmExplanation § Coordinates). Knows nothing of the logic (D104).
    /// </summary>
    public sealed class BlockView
    {
        private readonly List<PieceView> parts = new List<PieceView>();
        private readonly Material color;

        public Transform Root { get; }

        public BlockView(Transform root, Material color)
        {
            Root = root;
            this.color = color;
        }

        public void AddPart(PieceView part)
        {
            part.SetMaterial(color);
            parts.Add(part);
        }

        /// <summary>A modifier's view, placed under the root so it moves with the block.</summary>
        public T Attach<T>(T prefab) where T : Component => Object.Instantiate(prefab, Root);

        /// <summary>Draws every part with another material (Ice) instead of the block's palette color.</summary>
        public void SetSurface(Material material)
        {
            for (int i = 0; i < parts.Count; i++)
                parts[i].SetMaterial(material);
        }
    }
}

using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// Builds a <see cref="BlockView"/> for every block on the board: parts from the pool with their mesh swapped
    /// (D99), the block's palette material, and each modifier's look through its presenter (D104).
    /// Keeps the views by entity id so the drag (and later the director) can reach a block's view.
    /// </summary>
    public sealed class BlocksView
    {
        private readonly Transform root;
        private readonly PresentationAssets assets;
        private readonly PaletteMaterials materials;
        private readonly PiecePool parts;
        private readonly ModifierPresenters presenters;
        private readonly HashSet<Cell> cellBuffer = new HashSet<Cell>();
        private readonly List<BlockPart> partBuffer = new List<BlockPart>();
        private BlockView[] views;

        public BlocksView(Transform root, PresentationAssets assets, PaletteMaterials materials, PiecePool parts,
            ModifierPresenters presenters)
        {
            this.root = root;
            this.assets = assets;
            this.materials = materials;
            this.parts = parts;
            this.presenters = presenters;
        }

        public void Build(Board board)
        {
            views = new BlockView[board.EntityCount];

            for (int id = 0; id < board.EntityCount; id++)
            {
                if (board.GetEntity(id) is Block block)
                    views[id] = BuildBlock(block);
            }
        }

        public BlockView ViewOf(Block block) => views[block.Id];

        private BlockView BuildBlock(Block block)
        {
            var blockRoot = new GameObject($"Block_{block.Id}").transform;
            blockRoot.SetParent(root, false);

            Vector3 home = BoardLayout.ToBoard(new Vector2(block.Position.X, block.Position.Y), 0f);
            var view = new BlockView(blockRoot, materials.Block(Colors.Of(block)), home);
            AddParts(view, block);
            AddModifiers(view, block);

            return view;
        }

        private void AddParts(BlockView view, Block block)
        {
            cellBuffer.Clear();
            partBuffer.Clear();

            for (int i = 0; i < block.CellCount; i++)
                cellBuffer.Add(block.GetCell(i));

            BlockDrawRule.Build(cellBuffer, partBuffer);

            for (int i = 0; i < partBuffer.Count; i++)
            {
                BlockPart part = partBuffer[i];
                PieceView piece = parts.Get(view.Root);
                piece.SetMesh(assets.BlockMesh(part.Kind));
                piece.transform.SetLocalPositionAndRotation(part.Placement.Position,
                    Quaternion.Euler(0f, part.Placement.Yaw, 0f));
                view.AddPart(piece);
            }
        }

        private void AddModifiers(BlockView view, Block block)
        {
            for (int i = 0; i < block.ModifierCount; i++)
            {
                IModifier modifier = block.GetModifier(i);
                presenters.Find(modifier)?.Show(view, block, modifier);
            }
        }
    }
}

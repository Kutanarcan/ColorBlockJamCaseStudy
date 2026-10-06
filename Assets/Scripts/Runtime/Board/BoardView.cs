using System.Collections.Generic;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>Instantiates a <see cref="BoardDressing"/> under a root, setting each piece through its view (D99).</summary>
    public sealed class BoardView
    {
        private readonly Transform root;
        private readonly PresentationAssets assets;
        private readonly PaletteMaterials materials;

        public BoardView(Transform root, PresentationAssets assets, PaletteMaterials materials)
        {
            this.root = root;
            this.assets = assets;
            this.materials = materials;
        }

        public void Build(BoardDressing dressing)
        {
            for (int i = 0; i < dressing.Ground.Count; i++)
                Spawn(assets.GroundTile, dressing.Ground[i]);

            SpawnWalls(dressing.Walls, assets.WallMesh);
            SpawnWalls(dressing.Corners, assets.CornerMesh);

            for (int i = 0; i < dressing.Doors.Count; i++)
            {
                DoorRun run = dressing.Doors[i];
                DoorPieceView door = Spawn(assets.DoorPiece, run.Placement);
                door.SetLength(run.Length);
                door.SetMaterial(materials.Door(run.ColorId));
            }
        }

        private void SpawnWalls(IReadOnlyList<Placement> placements, Mesh mesh)
        {
            for (int i = 0; i < placements.Count; i++)
                Spawn(assets.WallPiece, placements[i]).SetMesh(mesh);
        }

        private T Spawn<T>(T prefab, Placement placement) where T : Component
        {
            T piece = Object.Instantiate(prefab, root);
            piece.transform.SetLocalPositionAndRotation(placement.Position, Quaternion.Euler(0f, placement.Yaw, 0f));

            return piece;
        }
    }
}

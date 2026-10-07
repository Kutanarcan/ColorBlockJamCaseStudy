using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Runtime
{
    /// <summary>
    /// Instantiates a <see cref="BoardDressing"/> under a root, setting each piece through its view (D99).
    /// Walls and door arrows draw with instanced copies of the kit's materials (the kit's own are embedded in its
    /// FBX and cannot be switched to instancing); the owner disposes them with the level.
    /// </summary>
    public sealed class BoardView : IDisposable
    {
        private readonly Transform root;
        private readonly PresentationAssets assets;
        private readonly PaletteMaterials materials;
        private readonly Material wallMaterial;
        private readonly Material doorArrowMaterial;

        public BoardView(Transform root, PresentationAssets assets, PaletteMaterials materials)
        {
            this.root = root;
            this.assets = assets;
            this.materials = materials;
            wallMaterial = Instanced(assets.WallPiece.Renderer.sharedMaterial, "Wall_Instanced");
            doorArrowMaterial = Instanced(assets.DoorPiece.ArrowRenderer.sharedMaterial, "DoorArrow_Instanced");
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
                door.SetArrowMaterial(doorArrowMaterial);
            }
        }

        public void Dispose()
        {
            Object.Destroy(wallMaterial);
            Object.Destroy(doorArrowMaterial);
        }

        private static Material Instanced(Material template, string name) =>
            new Material(template) { name = name, enableInstancing = true };

        private void SpawnWalls(IReadOnlyList<Placement> placements, Mesh mesh)
        {
            for (int i = 0; i < placements.Count; i++)
            {
                PieceView wall = Spawn(assets.WallPiece, placements[i]);
                wall.SetMesh(mesh);
                wall.SetMaterial(wallMaterial);
            }
        }

        private T Spawn<T>(T prefab, Placement placement) where T : Component
        {
            T piece = Object.Instantiate(prefab, root);
            piece.transform.SetLocalPositionAndRotation(placement.Position, Quaternion.Euler(0f, placement.Yaw, 0f));

            return piece;
        }
    }
}

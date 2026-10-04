using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Prototype
{
    [Serializable]
    public class BlockData
    {
        public int color;
        public List<Vector2Int> cells = new List<Vector2Int>();
    }

    public class Board : MonoBehaviour
    {
        const float CellSize = 2f;

        [SerializeField] int width = 6;
        [SerializeField] int height = 8;
        [SerializeField] GameObject groundTilePrefab;
        [SerializeField, Range(0f, 1f)] float cellGap = 0.1f; // added to the 2-unit pitch; the mesh already leaves 0.047

        [Header("Blocks")]
        [SerializeField] GameObject blockPiecePrefab;
        [SerializeField] Mesh outerCornerMesh;
        [SerializeField] Color[] palette =
        {
            new Color(0.91f, 0.27f, 0.27f), // red
            new Color(0.25f, 0.52f, 0.95f), // blue
            new Color(0.30f, 0.80f, 0.35f), // green
            new Color(0.98f, 0.82f, 0.20f), // yellow
            new Color(0.62f, 0.36f, 0.88f), // purple
            new Color(0.98f, 0.55f, 0.18f), // orange
        };

        [Header("Level")]
        [SerializeField] List<BlockData> blocks = new List<BlockData>
        {
            new BlockData { color = 0, cells = new List<Vector2Int> { new Vector2Int(2, 3) } },
        };

        float Pitch => CellSize + cellGap;

        Transform tiles;
        readonly Dictionary<int, Material> colorMaterials = new Dictionary<int, Material>();

        void Start()
        {
            BuildTiles();
            BuildBlocks();
        }

        void BuildTiles()
        {
            tiles = new GameObject("Tiles").transform;
            tiles.SetParent(transform, false);

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var tile = Instantiate(groundTilePrefab, tiles);
                tile.name = $"Tile_{x}_{y}";
                tile.transform.localPosition = CellCenter(x, y);
            }
        }

        void BuildBlocks()
        {
            for (int id = 0; id < blocks.Count; id++)
            {
                var data = blocks[id];
                var root = new GameObject($"Block_{id}").transform;
                root.SetParent(transform, false);

                // Phase 2: every cell is dressed as a lone 1x1 (4 x OuterCorner). BlockDrawRule replaces this in Phase 3.
                foreach (var cell in data.cells)
                    for (int rot = 0; rot < 360; rot += 90)
                        SpawnPiece(root, outerCornerMesh, CellCenter(cell.x, cell.y), rot, data.color, $"OuterCorner_{cell.x}_{cell.y}_r{rot}");
            }
        }

        void SpawnPiece(Transform parent, Mesh mesh, Vector3 localPos, float yRot, int color, string pieceName)
        {
            var piece = Instantiate(blockPiecePrefab, parent);
            piece.name = pieceName;
            piece.transform.localPosition = localPos;
            piece.transform.localRotation = Quaternion.Euler(0f, yRot, 0f);

            piece.GetComponentInChildren<MeshFilter>().sharedMesh = mesh;
            var renderer = piece.GetComponentInChildren<MeshRenderer>();
            renderer.sharedMaterial = ColorMaterial(color, renderer.sharedMaterial);
        }

        Material ColorMaterial(int color, Material template)
        {
            if (!colorMaterials.TryGetValue(color, out var mat))
            {
                mat = new Material(template) { name = $"Color_{color}", color = palette[color] };
                colorMaterials[color] = mat;
            }
            return mat;
        }

        // Board origin = bottom-left corner of cell (0, 0).
        public Vector3 CellCenter(int x, int y)
        {
            return new Vector3(x * Pitch + Pitch * 0.5f, 0f, y * Pitch + Pitch * 0.5f);
        }
    }
}
